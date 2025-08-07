using Microsoft.AspNetCore.Mvc;
using ETicaretSitesi.models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System;

namespace ETicaretSitesi.Controllers
{
    // Session için yardımcı extension
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        // HomeController ile uyumlu olması için eklenen metodlar
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    public class SepetController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public SepetController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        // Sepet sayfası
        public IActionResult Index()
        {
            var sepet = HttpContext.Session.GetObject<List<Urun>>("Sepet") ?? new List<Urun>();
            return View(sepet);
        }

        // Sepete ürün ekle
        [HttpPost]
        public IActionResult Ekle(int id)
        {
            var urun = _context.Urunler.FirstOrDefault(u => u.Id == id);
            var sepet = HttpContext.Session.GetObject<List<Urun>>("Sepet") ?? new List<Urun>();
            if (urun != null)
            {
                sepet.Add(urun);
                HttpContext.Session.SetObject("Sepet", sepet);
            }
            return RedirectToAction("Index", "Sepet");
        }

        // Sepetten ürün çıkar
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

                // Session-based cart system kullan
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

        // Sepet ürün adetini güncelle
        [HttpPost]
        public IActionResult AdetGuncelle(int sepetId, int adet)
        {
            // Session-based cart system kullan
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            // Debug bilgisi
            System.Diagnostics.Debug.WriteLine($"AdetGuncelle çağrıldı - sepetId: {sepetId}, adet: {adet}");

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

            // Eski adetleri kaldır
            sepet.RemoveAll(x => x == sepetId);

            // Yeni adet ekle (0'dan büyükse)
            if (adet > 0)
            {
                for (int i = 0; i < adet; i++)
                    sepet.Add(sepetId);
            }

            // Session'ı güncelle
            HttpContext.Session.SetObject("Sepet", sepet);

            System.Diagnostics.Debug.WriteLine($"Adet güncellendi: {adet}, yeni sepet eleman sayısı: {sepet.Count}");

            TempData["Mesaj"] = "Ürün adedi güncellendi.";
            TempData["MesajTipi"] = "success";

            return RedirectToAction("Sepetim", "Home");
        }

        // Sipariş oluştur ve ödeme sayfasına yönlendir
        [HttpGet]
        public IActionResult SiparisOlustur()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciId))
            {
                TempData["Mesaj"] = "Sipariş vermek için giriş yapmalısınız.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Login", "Auth");
            }

            // Session-based cart system kullan
            List<int> sepetList = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            if (!sepetList.Any())
            {
                TempData["Mesaj"] = "Sepetinizde ürün bulunmamaktadır.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Sepetim", "Home");
            }

            // Sepet ürünlerini oluştur
            var sepetUrunleri = sepetList
                .GroupBy(id => id)
                .Select(g => new models.Sepet
                {
                    Id = g.Key,
                    Urun = _context.Urunler.Include(u => u.Kategori).FirstOrDefault(u => u.Id == g.Key),
                    Adet = g.Count()
                }).ToList();

            // Sipariş oluşturma sayfasına yönlendir
            return View("SiparisOlustur", sepetUrunleri);
        }

        [HttpPost]
        public IActionResult SiparisOlustur(string adres)
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciId))
            {
                TempData["Mesaj"] = "Sipariş vermek için giriş yapmalısınız.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(adres))
            {
                TempData["Mesaj"] = "Lütfen teslimat adresini girin.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("SiparisOlustur");
            }

            // Session-based cart system kullan
            List<int> sepetList = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            if (!sepetList.Any())
            {
                TempData["Mesaj"] = "Sepetinizde ürün bulunmamaktadır.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Sepetim", "Home");
            }

            // Sepet ürünlerini oluştur
            var sepetUrunleri = sepetList
                .GroupBy(id => id)
                .Select(g => new models.Sepet
                {
                    Id = g.Key,
                    Urun = _context.Urunler.Include(u => u.Kategori).FirstOrDefault(u => u.Id == g.Key),
                    Adet = g.Count()
                }).ToList();

            // Toplam tutarı hesapla
            decimal toplamTutar = sepetUrunleri.Sum(s => s.Urun.Fiyat * s.Adet);

            // Yeni sipariş oluştur
            var siparis = new Siparis
            {
                KullaniciId = int.Parse(kullaniciId),
                SiparisTarihi = DateTime.Now,
                ToplamTutar = toplamTutar,
                Durum = "Beklemede",
                Adres = adres,
                OdemeDurumu = "Beklemede"
            };

            _context.Siparis.Add(siparis);
            _context.SaveChanges();



            // Session'daki sepeti temizle
            HttpContext.Session.Remove("Sepet");
            _context.SaveChanges();

            // Ödeme sayfasına yönlendir
            return RedirectToAction("OdemeSayfasi", "Odeme", new { siparisId = siparis.Id });
        }

        // Geçici test metodu
        [HttpGet]
        public IActionResult TestSepet()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");

            // Tüm sepet verilerini kontrol et
            var tumSepetler = _context.Sepet
                .Include(s => s.Urun)
                .ToList();

            var kullaniciSepeti = _context.Sepet
                .Include(s => s.Urun)
                .Where(s => s.KullaniciId == int.Parse(kullaniciId))
                .ToList();

            return Json(new
            {
                kullaniciId = kullaniciId,
                kullaniciIdInt = int.Parse(kullaniciId),
                tumSepetSayisi = tumSepetler.Count,
                kullaniciSepetSayisi = kullaniciSepeti.Count,
                tumSepetler = tumSepetler.Select(s => new {
                    id = s.Id,
                    kullaniciId = s.KullaniciId,
                    urunId = s.UrunId,
                    urunAdi = s.Urun.Ad,
                    adet = s.Adet,
                    fiyat = s.Urun.Fiyat
                }),
                kullaniciSepeti = kullaniciSepeti.Select(s => new {
                    urunAdi = s.Urun.Ad,
                    adet = s.Adet,
                    fiyat = s.Urun.Fiyat
                })
            });
        }

        // Test adet güncelleme
        [HttpGet]
        public IActionResult TestAdetGuncelle(int urunId, int yeniAdet)
        {
            // Session-based cart system kullan
            List<int> sepet = HttpContext.Session.GetObject<List<int>>("Sepet") ?? new List<int>();

            // Ürünün mevcut adetini kontrol et
            int mevcutAdet = sepet.Count(x => x == urunId);

            if (mevcutAdet > 0)
            {
                // Eski adetleri kaldır
                sepet.RemoveAll(x => x == urunId);

                // Yeni adet ekle (0'dan büyükse)
                if (yeniAdet > 0)
                {
                    for (int i = 0; i < yeniAdet; i++)
                        sepet.Add(urunId);
                }

                // Session'ı güncelle
                HttpContext.Session.SetObject("Sepet", sepet);

                // Ürün bilgisini al
                var urun = _context.Urunler.FirstOrDefault(u => u.Id == urunId);
                string urunAdi = urun?.Ad ?? "Bilinmeyen Ürün";

                return Json(new
                {
                    success = true,
                    message = $"Adet güncellendi: {yeniAdet}",
                    urunAdi = urunAdi
                });
            }

            return Json(new
            {
                success = false,
                message = "Bu ürün sepetinizde bulunamadı"
            });
        }

        // Sepeti temizle
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
    }
}