using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Odeme
    {
        public int Id { get; set; }
        public int SiparisId { get; set; }
        public string OdemeYontemi { get; set; } // "Kredi Kartı", "Havale", "Kapıda Ödeme"
        public decimal Tutar { get; set; }
        public string Durum { get; set; } // "Beklemede", "Başarılı", "Başarısız"
        public DateTime OdemeTarihi { get; set; }
        public string? IslemKodu { get; set; }
        public string? KartNumarasi { get; set; } // Maskelenmiş kart numarası
        public string? Aciklama { get; set; }

        // Navigation property
        public Siparis Siparis { get; set; }
    }
}