using Microsoft.AspNetCore.Mvc;
using ETicaretSitesi.models;
using ETicaretSitesi.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ETicaretSitesi.Controllers
{
    public class AuthController : Controller
    {
        private readonly ETicaretSitesiContext _context;

        public AuthController(ETicaretSitesiContext context)
        {
            _context = context;
        }

        // Login sayfası
        [HttpGet]
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (HttpContext.Session.GetString("KullaniciId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Home/Login.cshtml");
        }

        // Login işlemi
        [HttpPost]
        public IActionResult Login(string email, string sifre)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
                {
                    TempData["Mesaj"] = "Email ve şifre alanları boş olamaz.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                // Kullanıcıyı bul
                var kullanici = _context.Kullanici
                    .FirstOrDefault(k => k.Email.ToLower() == email.ToLower() && k.Aktif);

                if (kullanici == null)
                {
                    TempData["Mesaj"] = "Email veya şifre hatalı.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                // Şifreyi doğrula
                if (!PasswordHasher.VerifyPassword(sifre, kullanici.SifreHash))
                {
                    TempData["Mesaj"] = "Email veya şifre hatalı.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                // Session'a kullanıcı bilgilerini kaydet
                HttpContext.Session.SetString("KullaniciId", kullanici.Id.ToString());
                HttpContext.Session.SetString("KullaniciAd", kullanici.Ad);
                HttpContext.Session.SetString("KullaniciSoyad", kullanici.Soyad);
                HttpContext.Session.SetString("KullaniciEmail", kullanici.Email);

                TempData["Mesaj"] = $"Hoş geldiniz, {kullanici.Ad} {kullanici.Soyad}!";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Giriş yapılırken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return View();
            }
        }

        // Register sayfası
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("KullaniciId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Home/Register.cshtml");
        }

        // Register işlemi
        [HttpPost]
        public IActionResult Register(string ad, string soyad, string email, string sifre, string sifreTekrar, string telefon)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
                {
                    TempData["Mesaj"] = "Tüm zorunlu alanları doldurun.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                if (sifre != sifreTekrar)
                {
                    TempData["Mesaj"] = "Şifreler eşleşmiyor.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                if (!PasswordHasher.IsPasswordStrong(sifre))
                {
                    TempData["Mesaj"] = "Şifre en az 6 karakter olmalı ve harf + rakam içermelidir.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                // Email kontrolü
                if (_context.Kullanici.Any(k => k.Email.ToLower() == email.ToLower()))
                {
                    TempData["Mesaj"] = "Bu email adresi zaten kullanılıyor.";
                    TempData["MesajTipi"] = "danger";
                    return View();
                }

                // Yeni kullanıcı oluştur
                var yeniKullanici = new Kullanici
                {
                    Ad = ad.Trim(),
                    Soyad = soyad.Trim(),
                    Email = email.ToLower().Trim(),
                    SifreHash = PasswordHasher.HashPassword(sifre),
                    Telefon = telefon?.Trim(),
                    KayitTarihi = DateTime.Now,
                    Aktif = true
                };

                _context.Kullanici.Add(yeniKullanici);
                _context.SaveChanges();

                TempData["Mesaj"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                TempData["MesajTipi"] = "success";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Kayıt olurken bir hata oluştu.";
                TempData["MesajTipi"] = "danger";
                return View();
            }
        }

        // Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Mesaj"] = "Başarıyla çıkış yaptınız.";
            TempData["MesajTipi"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // Profil sayfası
        [HttpGet]
        public IActionResult Profile()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            if (kullaniciId == null)
            {
                return RedirectToAction("Login");
            }

            var kullanici = _context.Kullanici.FirstOrDefault(k => k.Id == int.Parse(kullaniciId));
            if (kullanici == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            return View("~/Views/Home/Profile.cshtml", kullanici);
        }
    }
}