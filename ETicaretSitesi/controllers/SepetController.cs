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

        
        [HttpGet]
        public IActionResult Test()
        {
            return Content("SepetController çalışıyor!");
        }

        
        [HttpPost]
        public IActionResult SepetiTemizle()
        {
            try
            {
               
                HttpContext.Session.Remove("Sepet");

               
                TempData["Mesaj"] = "Sepetiniz başarıyla temizlendi.";
                TempData["MesajTipi"] = "success";

                
                return RedirectToAction("Sepetim", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Sepet temizlenirken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Sepetim", "Home");
            }
        }

        
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

        
        [HttpGet]
        public IActionResult Odeme()
        {
            
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