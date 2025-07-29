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

        
        public IActionResult UrunDetay(int id)
        {
            var urun = _context.Urunler
                .Include(u => u.Kategori)
                .FirstOrDefault(u => u.Id == id);

            if (urun == null)
            {
                TempData["Mesaj"] = "Ürün bulunamadı.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("Index");
            }

            
            var yorumlar = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Where(y => y.UrunId == id && y.Onaylandi)
                .OrderByDescending(y => y.Tarih)
                .ToList();

            ViewBag.Yorumlar = yorumlar;

            
            ViewBag.OrtalamaPuan = yorumlar.Any() ? yorumlar.Average(y => y.Puan) : 0;

            return View(urun);
        }

        public IActionResult Sepetim(string action = "", int sepetId = 0, int adet = 0)
        {
            
            System.Diagnostics.Debug.WriteLine($"Sepetim çağrıldı: action={action}, sepetId={sepetId}, adet={adet}");

            
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