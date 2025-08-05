using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ETicaretSitesi.models
{
    public class SiparisDetay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SiparisId { get; set; }

        [Required]
        public int UrunId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Adet 1'den büyük olmalıdır.")]
        public int Adet { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        public decimal BirimFiyat { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Toplam fiyat 0'dan büyük olmalıdır.")]
        public decimal ToplamFiyat { get; set; }

        [ForeignKey("SiparisId")]
        public virtual Siparis Siparis { get; set; }

        [ForeignKey("UrunId")]
        public virtual Urun Urun { get; set; }

        [NotMapped]
        public decimal ToplamTutar => BirimFiyat * Adet;

        [NotMapped]
        public bool StokYeterli => Urun?.Stok >= Adet;

        public void ToplamFiyatHesapla()
        {
            ToplamFiyat = BirimFiyat * Adet;
        }

        public void BirimFiyatGuncelle(decimal yeniFiyat)
        {
            BirimFiyat = yeniFiyat;
            ToplamFiyatHesapla();
        }

        public void AdetGuncelle(int yeniAdet)
        {
            if (yeniAdet > 0)
            {
                Adet = yeniAdet;
                ToplamFiyatHesapla();
            }
        }
    }
}