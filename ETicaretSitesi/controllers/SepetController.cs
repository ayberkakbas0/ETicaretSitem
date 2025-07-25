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

        // Adet güncelle
        [HttpPost]
        public IActionResult AdetGuncelle(int sepetId, int adet)
        {
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            // Önce ürünü tamamen çıkar
            sepet.RemoveAll(x => x == sepetId);

            // Sonra yeni adet kadar ekle
            for (int i = 0; i < adet; i++)
                sepet.Add(sepetId);

            HttpContext.Session.SetObject("Sepet", sepet);
            return RedirectToAction("Sepetim", "Home");
        }

        // Ürün sil
        [HttpPost]
        public IActionResult Sil(int sepetId)
        {
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
            sepet.RemoveAll(x => x == sepetId);
            HttpContext.Session.SetObject("Sepet", sepet);
            return RedirectToAction("Sepetim", "Home");
        }

        // Sepeti temizle
        [HttpPost]
        public IActionResult SepetiTemizle()
        {
            HttpContext.Session.Remove("Sepet");
            return RedirectToAction("Sepetim", "Home");
        }

        // Ödeme (örnek)
        [HttpGet]
        public IActionResult Odeme()
        {
            // Burada ödeme sayfası açılabilir
            return View();
        }
    }
}