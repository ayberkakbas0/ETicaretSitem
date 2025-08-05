using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ETicaretSitesi.models
{
    public class Siparis
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KullaniciId { get; set; }

        [Required]
        public DateTime SiparisTarihi { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamTutar { get; set; }

        [Required]
        [MaxLength(50)]
        public string Durum { get; set; } = "Beklemede"; 

        [Required]
        [MaxLength(500)]
        public string Adres { get; set; }

        [MaxLength(20)]
        public string? OdemeDurumu { get; set; } 

        [MaxLength(50)]
        public string? OdemeYontemi { get; set; } 

        [MaxLength(50)]
        public string? TakipKodu { get; set; }

        public DateTime? OnayTarihi { get; set; }

        public int? OnaylayanAdminId { get; set; }

        [MaxLength(500)]
        public string? Notlar { get; set; }

        [ForeignKey("KullaniciId")]
        public virtual Kullanici Kullanici { get; set; }

        [ForeignKey("OnaylayanAdminId")]
        public virtual Kullanici? OnaylayanAdmin { get; set; }

        public virtual ICollection<SiparisDetay> SiparisDetaylari { get; set; } = new List<SiparisDetay>();

        [NotMapped]
        public bool OdemeTamamlandi => OdemeDurumu == "Tamamlandı";

        [NotMapped]
        public bool AdminOnaylandi => OnayTarihi.HasValue;

        [NotMapped]
        public bool Kargoda => Durum == "Kargoda";

        [NotMapped]
        public bool TeslimEdildi => Durum == "Teslim Edildi";

        [NotMapped]
        public bool IptalEdildi => Durum == "İptal Edildi";

        public void Onayla(int adminId)
        {
            Durum = "Onaylandı";
            OnayTarihi = DateTime.Now;
            OnaylayanAdminId = adminId;
        }

        public void KargoyaVer(string takipKodu)
        {
            Durum = "Kargoda";
            TakipKodu = takipKodu;
        }

        public void TeslimEt()
        {
            Durum = "Teslim Edildi";
        }

        public void IptalEt(string notlar = null)
        {
            Durum = "İptal Edildi";
            Notlar = notlar;
        }

        public void OdemeYap(string yontem)
        {
            OdemeYontemi = yontem;
            OdemeDurumu = "Tamamlandı";
        }
    }
}