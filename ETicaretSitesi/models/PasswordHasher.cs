using System.Security.Cryptography;
using System.Text;

namespace ETicaretSitesi.Utilities
{
    public static class PasswordHasher
    {
        // Şifreyi hashle
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Şifre doğrulama
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        // Güvenli şifre kontrolü
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }
    }
}