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

        public IActionResult Index(int? kategori, string arama)
        {
            var Urunler = _context.Urunler
                .Include(u => u.Kategori)
                .AsQueryable();

            // Kategori filtresi
            if (kategori.HasValue)
            {
                Urunler = Urunler.Where(u => u.KategoriId == kategori.Value);
            }

            // Arama filtresi
            if (!string.IsNullOrEmpty(arama))
            {
                Urunler = Urunler.Where(u =>
                    u.Ad.Contains(arama) ||
                    u.Aciklama.Contains(arama) ||
                    u.Kategori.Isim.Contains(arama));
            }

            ViewBag.Kategori = kategori;
            ViewBag.Arama = arama;
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

        // Ürün detay sayfası
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

            // Yorumları ayrıca getir
            var yorumlar = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Where(y => y.UrunId == id && y.Onaylandi)
                .OrderByDescending(y => y.Tarih)
                .ToList();

            ViewBag.Yorumlar = yorumlar;

            // Ortalama puan hesapla
            ViewBag.OrtalamaPuan = yorumlar.Any() ? yorumlar.Average(y => y.Puan) : 0;

            return View(urun);
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