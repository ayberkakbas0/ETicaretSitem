using ETicaretSitesi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        public IActionResult Sepetim()
        {
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            var sepetList = sepet
                .GroupBy(id => id)
                .Select(g => new Sepet
                {
                    Id = g.Key,
                    Urun = _context.Urunler.FirstOrDefault(u => u.Id == g.Key),
                    Adet = g.Count()
                }).ToList();

            decimal toplamTutar = sepetList.Sum(s => s.Urun.Fiyat * s.Adet);
            ViewBag.ToplamTutar = toplamTutar;

            return View(sepetList);
        }
        [HttpPost]
        public IActionResult SepeteEkle(int urunId)
        {
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();
            sepet.Add(urunId);
            HttpContext.Session.SetObject("Sepet", sepet);
            return RedirectToAction("Index");
        }
    }
}