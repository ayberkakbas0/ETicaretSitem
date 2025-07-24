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
        public string? Telefon { get; set; }
        public DateTime KayitTarihi { get; set; }
        public bool Aktif { get; set; }
    }
}