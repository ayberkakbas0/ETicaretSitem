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
                .Include(s => s.Kullanici)
                .FirstOrDefault(s => s.Id == siparisId && s.KullaniciId == int.Parse(kullaniciId));

            if (siparis == null)
            {
                return NotFound();
            }

            return View("~/Views/Home/OdemeSayfasi.cshtml", siparis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemeYap(int siparisId, string odemeYontemi, string kartNumarasi = null, string kartSahibi = null, string sonKullanmaTarihi = null, string cvv = null)
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var siparis = _context.Siparis
                .FirstOrDefault(s => s.Id == siparisId && s.KullaniciId == int.Parse(kullaniciId));

            if (siparis == null)
            {
                return NotFound();
            }

            
            bool odemeBasarili = SimulatePayment(odemeYontemi, kartNumarasi, cvv);

            if (odemeBasarili)
            {
                
                try
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
                }
                catch (Exception ex)
                {
                    
                    System.Diagnostics.Debug.WriteLine($"Odeme tablosu bulunamadı: {ex.Message}");
                }

                
                siparis.OdemeDurumu = "Tamamlandı";
                siparis.Durum = "Beklemede"; 

                
                

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    
                    System.Diagnostics.Debug.WriteLine($"SaveChanges hatası: {ex.Message}");
                }

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

        
        [HttpPost]
        public IActionResult TestOdeme(int siparisId, string odemeYontemi)
        {
            TempData["Basarili"] = $"Test ödeme: Sipariş {siparisId}, Yöntem: {odemeYontemi}";
            return RedirectToAction("OdemeSayfasi", new { siparisId });
        }
    }
} 