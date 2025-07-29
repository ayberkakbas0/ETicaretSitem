using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Siparis
    {
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string Durum { get; set; }
        public string Adres { get; set; }

        
        public Kullanici Kullanici { get; set; }
    }
}