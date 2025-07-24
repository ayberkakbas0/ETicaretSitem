using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicaretSitesi.models;

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
    }
}