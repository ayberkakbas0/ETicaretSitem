using Microsoft.AspNetCore.Mvc;
using ETicaretSitesi.models;
using ETicaretSitesi.Extensions;

namespace ETicaretSitesi.Controllers
{
    public class SepetController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public SepetController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        // Test action - GET
        [HttpGet]
        public IActionResult Test()
        {
            return Content("SepetController çalışıyor!");
        }

        // Sepeti temizle - Geliştirilmiş versiyon
        [HttpPost]
        public IActionResult SepetiTemizle()
        {
            try
            {
                // Session'ı temizle
                HttpContext.Session.Remove("Sepet");

                // Başarı mesajı ekle
                TempData["Mesaj"] = "Sepetiniz başarıyla temizlendi.";
                TempData["MesajTipi"] = "success";

                // Sepetim sayfasına yönlendir
                return RedirectToAction("Sepetim", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Sepet temizlenirken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Sepetim", "Home");
            }
        }

        // Ürün sil - Geliştirilmiş versiyon
        [HttpPost]
        public IActionResult Sil(int sepetId)
        {
            try
            {
                if (sepetId <= 0)
                {
                    TempData["Mesaj"] = "Geçersiz ürün ID'si.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Sepetim", "Home");
                }

                List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
                int eskiAdet = sepet.Count(x => x == sepetId);

                if (eskiAdet == 0)
                {
                    TempData["Mesaj"] = "Bu ürün sepetinizde bulunamadı.";
                    TempData["MesajTipi"] = "warning";
                    return RedirectToAction("Sepetim", "Home");
                }

                sepet.RemoveAll(x => x == sepetId);
                HttpContext.Session.SetObject("Sepet", sepet);

                TempData["Mesaj"] = "Ürün sepetten başarıyla silindi.";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("Sepetim", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Ürün silinirken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Sepetim", "Home");
            }
        }

        // Adet güncelle - Geliştirilmiş versiyon
        [HttpPost]
        public IActionResult AdetGuncelle(int sepetId, int adet)
        {
            try
            {
                if (sepetId <= 0 || adet <= 0)
                {
                    TempData["Mesaj"] = "Geçersiz parametreler.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Sepetim", "Home");
                }

                // Ürün stok kontrolü
                var urun = _context.Urunler.FirstOrDefault(u => u.Id == sepetId);
                if (urun == null)
                {
                    TempData["Mesaj"] = "Ürün bulunamadı.";
                    TempData["MesajTipi"] = "danger";
                    return RedirectToAction("Sepetim", "Home");
                }

                if (adet > urun.Stok)
                {
                    TempData["Mesaj"] = $"Bu üründen maksimum {urun.Stok} adet ekleyebilirsiniz.";
                    TempData["MesajTipi"] = "warning";
                    return RedirectToAction("Sepetim", "Home");
                }

                List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
                sepet.RemoveAll(x => x == sepetId);

                for (int i = 0; i < adet; i++)
                    sepet.Add(sepetId);

                HttpContext.Session.SetObject("Sepet", sepet);

                TempData["Mesaj"] = "Ürün adedi güncellendi.";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("Sepetim", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Adet güncellenirken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Sepetim", "Home");
            }
        }

        // Ödeme
        [HttpGet]
        public IActionResult Odeme()
        {
            // Sepet kontrolü
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
            if (!sepet.Any())
            {
                TempData["Mesaj"] = "Sepetinizde ürün bulunmuyor.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Sepetim", "Home");
            }

            return View();
        }
    }
}