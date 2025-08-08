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

            // Sahte ödeme işlemi simülasyonu
            bool odemeBasarili = SimulatePayment(odemeYontemi, kartNumarasi, cvv);

            if (odemeBasarili)
            {
                // Ödeme kaydı oluştur (veritabanı tablosu yoksa atla)
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
                    // Odeme tablosu yoksa bu hatayı görmezden gel
                    System.Diagnostics.Debug.WriteLine($"Odeme tablosu bulunamadı: {ex.Message}");
                }

                // Sipariş durumunu güncelle
                siparis.OdemeDurumu = "Tamamlandı";
                siparis.Durum = "Beklemede"; // Admin onayı bekliyor

                // Ürün stoklarını düşür
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Stok düşürme başladı. Sipariş ID: {siparisId}");
                    System.Diagnostics.Debug.WriteLine($"Sipariş notları: {siparis.Notlar}");

                    // Sipariş notlarından sepet bilgilerini al
                    if (!string.IsNullOrEmpty(siparis.Notlar))
                    {
                        var sepetDetaylari = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(siparis.Notlar);
                        System.Diagnostics.Debug.WriteLine($"Sepet detayları deserialize edildi. Ürün sayısı: {sepetDetaylari?.Count ?? 0}");

                        if (sepetDetaylari != null)
                        {
                            // Her ürün için stok düşür
                            foreach (var sepetUrun in sepetDetaylari)
                            {
                                int urunId = sepetUrun.GetProperty("UrunId").GetInt32();
                                int adet = sepetUrun.GetProperty("Adet").GetInt32();

                                System.Diagnostics.Debug.WriteLine($"İşlenen ürün: ID={urunId}, Adet={adet}");

                                var urun = _context.Urunler.FirstOrDefault(u => u.Id == urunId);
                                if (urun != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Ürün bulundu: {urun.Ad}, Mevcut stok: {urun.Stok}");

                                    if (urun.Stok >= adet)
                                    {
                                        int eskiStok = urun.Stok;
                                        urun.Stok -= adet;
                                        System.Diagnostics.Debug.WriteLine($"Ürün {urun.Ad} stoku {eskiStok} -> {urun.Stok} olarak güncellendi");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Ürün {urun.Ad} için yetersiz stok: {urun.Stok} < {adet}");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Ürün bulunamadı: ID={urunId}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Sepet detayları null");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Sipariş notları boş");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Stok düşürme hatası: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Hata detayı: {ex.StackTrace}");
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine("SaveChanges başlatılıyor...");
                    int affectedRows = _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"SaveChanges tamamlandı. Etkilenen satır sayısı: {affectedRows}");
                }
                catch (Exception ex)
                {
                    // Odeme tablosu yoksa bu hatayı görmezden gel
                    System.Diagnostics.Debug.WriteLine($"SaveChanges hatası: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SaveChanges hata detayı: {ex.StackTrace}");
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
            // Sahte ödeme simülasyonu
            // Gerçek uygulamada bu kısım gerçek ödeme sağlayıcısı API'si ile değiştirilir

            if (odemeYontemi == "Kapıda Ödeme")
            {
                return true; // Kapıda ödeme her zaman başarılı
            }

            if (string.IsNullOrEmpty(kartNumarasi) || string.IsNullOrEmpty(cvv))
            {
                return false;
            }

            // Test kart numaraları
            if (kartNumarasi.StartsWith("4111") || kartNumarasi.StartsWith("5555"))
            {
                return true; // Başarılı ödeme
            }
            else if (kartNumarasi.StartsWith("4000"))
            {
                return false; // Başarısız ödeme
            }

            // Rastgele başarı oranı (%90 başarılı)
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

        // Debug için test metodu
        [HttpPost]
        public IActionResult TestOdeme(int siparisId, string odemeYontemi)
        {
            TempData["Basarili"] = $"Test ödeme: Sipariş {siparisId}, Yöntem: {odemeYontemi}";
            return RedirectToAction("OdemeSayfasi", new { siparisId });
        }

        // Stok test metodu
        [HttpPost]
        public IActionResult TestStokDusur(int siparisId)
        {
            try
            {
                var siparis = _context.Siparis.FirstOrDefault(s => s.Id == siparisId);
                if (siparis == null)
                {
                    TempData["Hata"] = "Sipariş bulunamadı.";
                    return RedirectToAction("Siparislerim", "Home");
                }

                System.Diagnostics.Debug.WriteLine($"Test stok düşürme başladı. Sipariş ID: {siparisId}");
                System.Diagnostics.Debug.WriteLine($"Sipariş notları: {siparis.Notlar}");

                if (!string.IsNullOrEmpty(siparis.Notlar))
                {
                    var sepetDetaylari = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(siparis.Notlar);
                    System.Diagnostics.Debug.WriteLine($"Sepet detayları: {sepetDetaylari?.Count ?? 0} ürün");

                    if (sepetDetaylari != null)
                    {
                        foreach (var sepetUrun in sepetDetaylari)
                        {
                            int urunId = sepetUrun.GetProperty("UrunId").GetInt32();
                            int adet = sepetUrun.GetProperty("Adet").GetInt32();

                            var urun = _context.Urunler.FirstOrDefault(u => u.Id == urunId);
                            if (urun != null)
                            {
                                int eskiStok = urun.Stok;
                                urun.Stok -= adet;
                                System.Diagnostics.Debug.WriteLine($"Ürün {urun.Ad}: {eskiStok} -> {urun.Stok}");
                            }
                        }

                        _context.SaveChanges();
                        TempData["Basarili"] = "Test stok düşürme başarılı! Debug loglarını kontrol edin.";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test stok düşürme hatası: {ex.Message}");
                TempData["Hata"] = $"Test hatası: {ex.Message}";
            }

            return RedirectToAction("Siparislerim", "Home");
        }
    }
}