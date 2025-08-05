using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Odeme
    {
        public int Id { get; set; }
        public int SiparisId { get; set; }
        public string OdemeYontemi { get; set; } 
        public decimal Tutar { get; set; }
        public string Durum { get; set; } 
        public DateTime OdemeTarihi { get; set; }
        public string? IslemKodu { get; set; }
        public string? KartNumarasi { get; set; } 
        public string? Aciklama { get; set; }

        
        public Siparis Siparis { get; set; }
    }
}