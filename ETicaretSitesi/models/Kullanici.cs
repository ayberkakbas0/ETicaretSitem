using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Email { get; set; }
        public string SifreHash { get; set; }

        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir")]
        [RegularExpression(@"^[0-9+\-\s\(\)]+$", ErrorMessage = "Geçerli bir telefon numarası girin")]
        public string? Telefon { get; set; }

        public DateTime KayitTarihi { get; set; }
        public bool Aktif { get; set; }

        
        public List<Yorum> Yorumlar { get; set; } = new List<Yorum>();
    }
}