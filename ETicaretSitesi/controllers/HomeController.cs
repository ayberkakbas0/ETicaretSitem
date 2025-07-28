using ETicaretSitesi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicaretSitesi.models;
using System.Linq;

namespace ETicaretSitesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public HomeController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? kategori)
        {
            var Urunler = _context.Urunler
                .Include(u => u.Kategori)
                .AsQueryable();

            if (kategori.HasValue)
            {
                Urunler = Urunler.Where(u => u.KategoriId == kategori.Value);
            }

            ViewBag.Kategori = kategori;
            ViewBag.Kategoriler = _context.Kategori.ToList();
            return View(Urunler.ToList());
        }

        [HttpPost]
        public IActionResult SepeteEkle(int urunId)
        {
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
            sepet.Add(urunId);
            HttpContext.Session.SetObject("Sepet", sepet);
            return RedirectToAction("Index");
        }

        public IActionResult Sepetim(string action = "", int sepetId = 0, int adet = 0)
        {
            // Debug için
            System.Diagnostics.Debug.WriteLine($"Sepetim çağrıldı: action={action}, sepetId={sepetId}, adet={adet}");

            // Sepet işlemleri
            if (!string.IsNullOrEmpty(action))
            {
                List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
                System.Diagnostics.Debug.WriteLine($"İşlem öncesi sepet eleman sayısı: {sepet.Count}");

                switch (action.ToLower())
                {
                    case "sil":
                        if (sepetId > 0)
                        {
                            int silinenAdet = sepet.RemoveAll(x => x == sepetId);
                            HttpContext.Session.SetObject("Sepet", sepet);
                            System.Diagnostics.Debug.WriteLine($"Sil işlemi: sepetId={sepetId}, silinen adet={silinenAdet}, yeni eleman sayısı: {sepet.Count}");
                        }
                        break;

                    case "adetguncelle":
                        if (sepetId > 0 && adet > 0)
                        {
                            sepet.RemoveAll(x => x == sepetId);
                            for (int i = 0; i < adet; i++)
                                sepet.Add(sepetId);
                            HttpContext.Session.SetObject("Sepet", sepet);
                            System.Diagnostics.Debug.WriteLine($"Adet güncelleme: sepetId={sepetId}, adet={adet}, yeni eleman sayısı: {sepet.Count}");
                        }
                        break;

                    case "temizle":
                        HttpContext.Session.Remove("Sepet");
                        System.Diagnostics.Debug.WriteLine("Sepet temizlendi");
                        break;
                }
            }

            // Sepet listesini getir
            List<int> sepetList = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
            System.Diagnostics.Debug.WriteLine($"Sepet listesi eleman sayısı: {sepetList.Count}");

            var sepetItems = sepetList
                .GroupBy(id => id)
                .Select(g => new Sepet
                {
                    Id = g.Key,
                    Urun = _context.Urunler.FirstOrDefault(u => u.Id == g.Key),
                    Adet = g.Count()
                }).ToList();

            decimal toplamTutar = sepetItems.Sum(s => s.Urun.Fiyat * s.Adet);
            ViewBag.ToplamTutar = toplamTutar;

            return View(sepetItems);
        }
    }
}