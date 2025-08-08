using Microsoft.AspNetCore.Mvc;
using ETicaretSitesi.models;
using ETicaretSitesi.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ETicaretSitesi.Controllers
{
    public class AdminController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public AdminController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        // Admin giriş kontrolü
        private bool IsAdmin()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (kullaniciId == null) return false;

            var kullanici = _context.Kullanici.FirstOrDefault(k => k.Id == int.Parse(kullaniciId));
            return kullanici != null && kullanici.AdminMi;
        }

        // Admin kayıt sayfası (sadece ilk admin için)
        [HttpGet]
        public IActionResult AdminKayit()
        {
            // Eğer zaten admin varsa, kayıt sayfasını engelle
            if (_context.Kullanici.Any(k => k.AdminMi))
            {
                TempData["Mesaj"] = "Admin zaten mevcut. Admin kaydı yapılamaz.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("AdminLogin");
            }

            return View("~/Views/Home/AdminKayit.cshtml");
        }

        // Admin kayıt işlemi
        [HttpPost]
        public IActionResult AdminKayit(string ad, string soyad, string email, string sifre, string sifreTekrar, string adminKodu)
        {
            try
            {
                // Eğer zaten admin varsa, kayıt engelle
                if (_context.Kullanici.Any(k => k.AdminMi))
                {
                    TempData["Mesaj"] = "Admin zaten mevcut. Admin kaydı yapılamaz.";
                    TempData["MesajTipi"] = "warning";
                    return RedirectToAction("AdminLogin");
                }

                // Validation
                if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
                {
                    TempData["Mesaj"] = "Tüm zorunlu alanları doldurun.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminKayit.cshtml");
                }

                if (sifre != sifreTekrar)
                {
                    TempData["Mesaj"] = "Şifreler eşleşmiyor.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminKayit.cshtml");
                }

                // Admin kodu kontrolü (basit güvenlik)
                if (adminKodu != "ADMIN2024")
                {
                    TempData["Mesaj"] = "Geçersiz admin kodu.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminKayit.cshtml");
                }

                // Email kontrolü
                if (_context.Kullanici.Any(k => k.Email.ToLower() == email.ToLower()))
                {
                    TempData["Mesaj"] = "Bu email adresi zaten kullanılıyor.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminKayit.cshtml");
                }

                // Yeni admin oluştur
                var yeniAdmin = new Kullanici
                {
                    Ad = ad.Trim(),
                    Soyad = soyad.Trim(),
                    Email = email.ToLower().Trim(),
                    SifreHash = PasswordHasher.HashPassword(sifre),
                    KayitTarihi = DateTime.Now,
                    Aktif = true,
                    AdminMi = true // Admin işaretle
                };

                _context.Kullanici.Add(yeniAdmin);
                _context.SaveChanges();

                TempData["Mesaj"] = "Admin hesabı başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("AdminLogin");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Admin kaydı olurken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return View("~/Views/Home/AdminKayit.cshtml");
            }
        }

        // Admin giriş sayfası
        [HttpGet]
        public IActionResult AdminLogin()
        {
            // Zaten giriş yapmışsa admin paneline yönlendir
            if (IsAdmin())
            {
                return RedirectToAction("AdminPanel");
            }

            return View("~/Views/Home/AdminLogin.cshtml");
        }

        // Admin giriş işlemi
        [HttpPost]
        public IActionResult AdminLogin(string email, string sifre)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
                {
                    TempData["Mesaj"] = "Email ve şifre alanları boş olamaz.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminLogin.cshtml");
                }

                // Admin kullanıcıyı bul
                var admin = _context.Kullanici
                    .FirstOrDefault(k => k.Email.ToLower() == email.ToLower() && k.AdminMi);

                if (admin == null)
                {
                    TempData["Mesaj"] = "Admin hesabı bulunamadı.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminLogin.cshtml");
                }

                // Şifreyi doğrula
                if (!PasswordHasher.VerifyPassword(sifre, admin.SifreHash))
                {
                    TempData["Mesaj"] = "Email veya şifre hatalı.";
                    TempData["MesajTipi"] = "danger";
                    return View("~/Views/Home/AdminLogin.cshtml");
                }

                // Session'a admin bilgilerini kaydet
                HttpContext.Session.SetString("KullaniciId", admin.Id.ToString());
                HttpContext.Session.SetString("KullaniciAd", admin.Ad);
                HttpContext.Session.SetString("KullaniciSoyad", admin.Soyad);
                HttpContext.Session.SetString("KullaniciEmail", admin.Email);
                HttpContext.Session.SetString("AdminMi", "true");

                TempData["Mesaj"] = $"Hoş geldiniz, Admin {admin.Ad} {admin.Soyad}!";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("AdminPanel");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Giriş yapılırken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return View("~/Views/Home/AdminLogin.cshtml");
            }
        }

        // Admin paneli ana sayfa
        [HttpGet]
        public IActionResult AdminPanel()
        {
            if (!IsAdmin())
            {
                TempData["Mesaj"] = "Bu sayfaya erişim yetkiniz yok.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("AdminLogin");
            }

            // İstatistikleri hesapla
            var toplamKullanici = _context.Kullanici.Count();
            var toplamUrun = _context.Urunler.Count();
            var toplamYorum = _context.Yorumlar.Count();
            var onayliYorum = _context.Yorumlar.Count(y => y.Onaylandi);
            var bekleyenYorum = _context.Yorumlar.Count(y => !y.Onaylandi);

            ViewBag.ToplamKullanici = toplamKullanici;
            ViewBag.ToplamUrun = toplamUrun;
            ViewBag.ToplamYorum = toplamYorum;
            ViewBag.OnayliYorum = onayliYorum;
            ViewBag.BekleyenYorum = bekleyenYorum;

            return View("~/Views/Home/AdminPanel.cshtml");
        }

        // Yorum yönetimi sayfası
        [HttpGet]
        public IActionResult YorumYonetimi()
        {
            if (!IsAdmin())
            {
                TempData["Mesaj"] = "Bu sayfaya erişim yetkiniz yok.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("AdminLogin");
            }

            var yorumlar = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Include(y => y.Urun)
                .OrderByDescending(y => y.Tarih)
                .ToList();

            return View("~/Views/Home/YorumYonetimi.cshtml", yorumlar);
        }

        // Yorum onaylama
        [HttpPost]
        public IActionResult YorumOnayla(int yorumId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var yorum = _context.Yorumlar.FirstOrDefault(y => y.Id == yorumId);
                if (yorum == null)
                {
                    return Json(new { success = false, message = "Yorum bulunamadı." });
                }

                yorum.Onaylandi = true;
                _context.SaveChanges();

                return Json(new { success = true, message = "Yorum onaylandı." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Yorum reddetme
        [HttpPost]
        public IActionResult YorumReddet(int yorumId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var yorum = _context.Yorumlar.FirstOrDefault(y => y.Id == yorumId);
                if (yorum == null)
                {
                    return Json(new { success = false, message = "Yorum bulunamadı." });
                }

                yorum.Onaylandi = false;
                _context.SaveChanges();

                return Json(new { success = true, message = "Yorum reddedildi." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Yorum silme (admin)
        [HttpPost]
        public IActionResult YorumSil(int yorumId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var yorum = _context.Yorumlar.FirstOrDefault(y => y.Id == yorumId);
                if (yorum == null)
                {
                    return Json(new { success = false, message = "Yorum bulunamadı." });
                }

                _context.Yorumlar.Remove(yorum);
                _context.SaveChanges();

                return Json(new { success = true, message = "Yorum silindi." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Kullanıcı yönetimi
        [HttpGet]
        public IActionResult KullaniciYonetimi()
        {
            if (!IsAdmin())
            {
                TempData["Mesaj"] = "Bu sayfaya erişim yetkiniz yok.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("AdminLogin");
            }

            var kullanicilar = _context.Kullanici
                .OrderByDescending(k => k.KayitTarihi)
                .ToList();

            return View("~/Views/Home/KullaniciYonetimi.cshtml", kullanicilar);
        }

        // Kullanıcı aktif yapma
        [HttpPost]
        public IActionResult KullaniciAktifYap(int kullaniciId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var kullanici = _context.Kullanici.FirstOrDefault(k => k.Id == kullaniciId);
                if (kullanici == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                kullanici.Aktif = true;
                _context.SaveChanges();

                return Json(new { success = true, message = "Kullanıcı aktif yapıldı." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Kullanıcı pasif yapma
        [HttpPost]
        public IActionResult KullaniciPasifYap(int kullaniciId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var kullanici = _context.Kullanici.FirstOrDefault(k => k.Id == kullaniciId);
                if (kullanici == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                kullanici.Aktif = false;
                _context.SaveChanges();

                return Json(new { success = true, message = "Kullanıcı pasif yapıldı." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Kullanıcı silme
        [HttpPost]
        public IActionResult KullaniciSil(int kullaniciId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Yetkisiz erişim." });
            }

            try
            {
                var kullanici = _context.Kullanici.FirstOrDefault(k => k.Id == kullaniciId);
                if (kullanici == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                if (kullanici.AdminMi)
                {
                    return Json(new { success = false, message = "Admin kullanıcıları silinemez." });
                }

                _context.Kullanici.Remove(kullanici);
                _context.SaveChanges();

                return Json(new { success = true, message = "Kullanıcı silindi." });
            }
            catch
            {
                return Json(new { success = false, message = "İşlem başarısız." });
            }
        }

        // Admin çıkış
        [HttpGet]
        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();
            TempData["Mesaj"] = "Admin panelinden çıkış yaptınız.";
            TempData["MesajTipi"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // Sipariş Yönetimi
        [HttpGet]
        public IActionResult SiparisYonetimi()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparisler = _context.Siparis
                .Include(s => s.Kullanici)
                .OrderByDescending(s => s.SiparisTarihi)
                .ToList();

            return View("~/Views/Home/SiparisYonetimi.cshtml", siparisler);
        }

        [HttpPost]
        public IActionResult SiparisOnayla(int siparisId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparis = _context.Siparis
                .FirstOrDefault(s => s.Id == siparisId);

            if (siparis == null)
            {
                TempData["Mesaj"] = "Sipariş bulunamadı.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("SiparisYonetimi");
            }

            // Siparişi onayla ve ödeme durumunu güncelle
            siparis.Durum = "Onaylandı";
            siparis.OdemeDurumu = "Ödendi";
            siparis.OnayTarihi = DateTime.Now;
            siparis.OnaylayanAdminId = int.Parse(HttpContext.Session.GetString("KullaniciId"));

            _context.SaveChanges();

            TempData["Mesaj"] = "Sipariş başarıyla onaylandı ve ödeme durumu güncellendi.";
            TempData["MesajTipi"] = "success";
            return RedirectToAction("SiparisYonetimi");
        }

        [HttpPost]
        public IActionResult SiparisDurumGuncelle(int siparisId, string yeniDurum)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparis = _context.Siparis.FirstOrDefault(s => s.Id == siparisId);
            if (siparis == null)
            {
                TempData["Mesaj"] = "Sipariş bulunamadı.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("SiparisYonetimi");
            }

            // Eğer sipariş "Onaylandı" durumuna geçiyorsa stokları düşür
            if (yeniDurum == "Onaylandı" && siparis.Durum != "Onaylandı")
            {
                try
                {
                    // Session tabanlı sepet sistemi olduğu için stok düşürme işlemi
                    // sadece ödeme tamamlandığında yapılır
                    // Burada sadece log kaydı tutuyoruz
                    System.Diagnostics.Debug.WriteLine($"Sipariş {siparisId} onaylandı - Stok düşürme işlemi ödeme sırasında yapıldı");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Stok düşürme hatası: {ex.Message}");
                }
            }

            // Eğer sipariş "Teslim Edildi" durumuna geçiyorsa ödeme durumunu "Ödendi" yap
            if (yeniDurum == "Teslim Edildi")
            {
                siparis.OdemeDurumu = "Ödendi";
            }

            siparis.Durum = yeniDurum;
            _context.SaveChanges();

            TempData["Mesaj"] = "Sipariş durumu güncellendi.";
            TempData["MesajTipi"] = "success";
            return RedirectToAction("SiparisYonetimi");
        }

        [HttpPost]
        public IActionResult SiparisIptalEt(int siparisId, string iptalNedeni)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparis = _context.Siparis
                .FirstOrDefault(s => s.Id == siparisId);

            if (siparis == null)
            {
                TempData["Mesaj"] = "Sipariş bulunamadı.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("SiparisYonetimi");
            }

            // Siparişi iptal et
            siparis.Durum = "İptal Edildi";
            siparis.Notlar = iptalNedeni;

            // Sipariş iptal edildi

            _context.SaveChanges();

            TempData["Mesaj"] = "Sipariş iptal edildi.";
            TempData["MesajTipi"] = "warning";
            return RedirectToAction("SiparisYonetimi");
        }

        [HttpPost]
        public IActionResult TakipKoduEkle(int siparisId, string takipKodu)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparis = _context.Siparis.FirstOrDefault(s => s.Id == siparisId);
            if (siparis == null)
            {
                TempData["Mesaj"] = "Sipariş bulunamadı.";
                TempData["MesajTipi"] = "danger";
                return RedirectToAction("SiparisYonetimi");
            }

            siparis.TakipKodu = takipKodu;
            siparis.Durum = "Kargoda";
            _context.SaveChanges();

            TempData["Mesaj"] = "Takip kodu eklendi ve sipariş kargoya verildi.";
            TempData["MesajTipi"] = "success";
            return RedirectToAction("SiparisYonetimi");
        }

        [HttpGet]
        public IActionResult SiparisDetay(int siparisId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin");
            }

            var siparis = _context.Siparis
                .Include(s => s.Kullanici)
                .FirstOrDefault(s => s.Id == siparisId);

            if (siparis == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/Home/SiparisDetayPartial.cshtml", siparis);
        }
    }
}