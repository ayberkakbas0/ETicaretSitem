using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicaretSitesi.models;
using System.Security.Cryptography;
using System.Text;

namespace ETicaretSitesi.Controllers
{
    public class OdemeController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public OdemeController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult OdemeSayfasi(int siparisId)
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var siparis = _context.Siparis
                .Include(s => s.SiparisDetaylari)
                .ThenInclude(sd => sd.Urun)
                .ThenInclude(u => u.Kategori)
                .Include(s => s.Kullanici)
                .FirstOrDefault(s => s.Id == siparisId && s.KullaniciId == int.Parse(kullaniciId));

            if (siparis == null)
            {
                return NotFound();
            }

            return View(siparis);
        }

        [HttpPost]
        public IActionResult OdemeYap(int siparisId, string odemeYontemi, string kartNumarasi = null, string kartSahibi = null, string sonKullanmaTarihi = null, string cvv = null)
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var siparis = _context.Siparis
                .Include(s => s.SiparisDetaylari)
                .ThenInclude(sd => sd.Urun)
                .FirstOrDefault(s => s.Id == siparisId && s.KullaniciId == int.Parse(kullaniciId));

            if (siparis == null)
            {
                return NotFound();
            }

            bool odemeBasarili = SimulatePayment(odemeYontemi, kartNumarasi, cvv);

            if (odemeBasarili)
            {
                var odeme = new Odeme
                {
                    SiparisId = siparisId,
                    OdemeYontemi = odemeYontemi,
                    Tutar = siparis.ToplamTutar,
                    Durum = "Başarılı",
                    OdemeTarihi = DateTime.Now,
                    IslemKodu = GenerateTransactionCode(),
                    KartNumarasi = !string.IsNullOrEmpty(kartNumarasi) ? MaskCardNumber(kartNumarasi) : null,
                    Aciklama = "Ödeme başarıyla tamamlandı"
                };

                _context.Odemeler.Add(odeme);

                siparis.OdemeDurumu = "Tamamlandı";
                siparis.Durum = "Beklemede"; 

                foreach (var detay in siparis.SiparisDetaylari)
                {
                    var urun = detay.Urun;
                    if (urun.Stok >= detay.Adet)
                    {
                        urun.Stok -= detay.Adet;
                    }
                    else
                    {
                        TempData["Hata"] = $"{urun.Ad} ürünü için yeterli stok bulunmamaktadır.";
                        return RedirectToAction("OdemeSayfasi", new { siparisId });
                    }
                }

                _context.SaveChanges();

                TempData["Basarili"] = "Ödeme başarıyla tamamlandı! Siparişiniz admin onayı beklemektedir.";
                return RedirectToAction("Siparislerim", "Home");
            }
            else
            {
                TempData["Hata"] = "Ödeme işlemi başarısız oldu. Lütfen bilgilerinizi kontrol edin.";
                return RedirectToAction("OdemeSayfasi", new { siparisId });
            }
        }

        private bool SimulatePayment(string odemeYontemi, string kartNumarasi, string cvv)
        {

            if (odemeYontemi == "Kapıda Ödeme")
            {
                return true; 
            }

            if (string.IsNullOrEmpty(kartNumarasi) || string.IsNullOrEmpty(cvv))
            {
                return false;
            }

            if (kartNumarasi.StartsWith("4111") || kartNumarasi.StartsWith("5555"))
            {
                return true; 
            }
            else if (kartNumarasi.StartsWith("4000"))
            {
                return false; 
            }

            Random random = new Random();
            return random.Next(1, 11) <= 9;
        }

        private string GenerateTransactionCode()
        {
            return "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            return "****-****-****-" + cardNumber.Substring(cardNumber.Length - 4);
        }
    }
}