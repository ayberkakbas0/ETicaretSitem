using Microsoft.AspNetCore.Mvc;
using ETicaretSitesi.models;
using Microsoft.EntityFrameworkCore;

namespace ETicaretSitesi.Controllers
{
    public class YorumController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public YorumController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        public IActionResult Ekle(int urunId, string metin, int puan)
        {
            try
            {
                
                var kullaniciId = HttpContext.Session.GetString("KullaniciId");
                if (kullaniciId == null)
                {
                    TempData["Mesaj"] = "Yorum yapmak için giriş yapmalısınız.";
                    TempData["MesajTipi"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                
                if (string.IsNullOrEmpty(metin?.Trim()))
                {
                    TempData["Mesaj"] = "Yorum metni boş olamaz.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Index", "Home");
                }

                if (puan < 1 || puan > 5)
                {
                    TempData["Mesaj"] = "Puan 1-5 arasında olmalıdır.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Index", "Home");
                }

                
                var urun = _context.Urunler.FirstOrDefault(u => u.Id == urunId);
                if (urun == null)
                {
                    TempData["Mesaj"] = "Ürün bulunamadı.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Index", "Home");
                }

                
                var oncekiYorum = _context.Yorumlar.FirstOrDefault(y =>
                    y.UrunId == urunId && y.KullaniciId == int.Parse(kullaniciId));

                if (oncekiYorum != null)
                {
                    TempData["Mesaj"] = "Bu ürün için zaten yorum yapmışsınız.";
                    TempData["MesajTipi"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                
                var yeniYorum = new Yorum
                {
                    UrunId = urunId,
                    KullaniciId = int.Parse(kullaniciId),
                    Metin = metin.Trim(),
                    Puan = puan,
                    Tarih = DateTime.Now,
                    Onaylandi = true 
                };

                _context.Yorumlar.Add(yeniYorum);
                _context.SaveChanges();

                TempData["Mesaj"] = "Yorumunuz başarıyla eklendi.";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Yorum eklenirken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Index", "Home");
            }
        }

        
        [HttpPost]
        public IActionResult Sil(int yorumId)
        {
            try
            {
                var kullaniciId = HttpContext.Session.GetString("KullaniciId");
                if (kullaniciId == null)
                {
                    return Json(new { success = false, message = "Giriş yapmalısınız." });
                }

                var yorum = _context.Yorumlar.FirstOrDefault(y =>
                    y.Id == yorumId && y.KullaniciId == int.Parse(kullaniciId));

                if (yorum == null)
                {
                    return Json(new { success = false, message = "Yorum bulunamadı." });
                }

                _context.Yorumlar.Remove(yorum);
                _context.SaveChanges();

                return Json(new { success = true, message = "Yorum silindi." });
            }
            catch
            {
                return Json(new { success = false, message = "Yorum silinirken hata oluştu." });
            }
        }

        
        [HttpGet]
        public IActionResult UrunYorumlari(int urunId)
        {
            var yorumlar = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Where(y => y.UrunId == urunId && y.Onaylandi)
                .OrderByDescending(y => y.Tarih)
                .ToList();

            return PartialView("_YorumlarPartial", yorumlar);
        }
    }
}