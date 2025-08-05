using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ETicaretSitesi.models
{
    public class Urun
    {
        [Key]
        [Column("UrunId")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Ad { get; set; }

        [MaxLength(1000)]
        public string Aciklama { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal Fiyat { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok 0'dan küçük olamaz.")]
        public int Stok { get; set; }

        [Required]
        public int KategoriId { get; set; }

        [MaxLength(500)]
        public string? ResimUrl { get; set; }

        [MaxLength(50)]
        public string? Marka { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }

        [MaxLength(20)]
        public string? Renk { get; set; }

        [MaxLength(20)]
        public string? Boyut { get; set; }

        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır.")]
        public decimal? IndirimOrani { get; set; }

        public bool Aktif { get; set; } = true;

        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

        public DateTime? GuncellemeTarihi { get; set; }

        [MaxLength(500)]
        public string? Etiketler { get; set; } 

        [Range(0, 5, ErrorMessage = "Puan 0-5 arasında olmalıdır.")]
        public decimal? VarsayilanPuan { get; set; }

        
        [ForeignKey("KategoriId")]
        public virtual Kategori Kategori { get; set; }

        public virtual ICollection<Yorum> Yorumlar { get; set; } = new List<Yorum>();

        public virtual ICollection<SiparisDetay> SiparisDetaylari { get; set; } = new List<SiparisDetay>();

        [NotMapped]
        public decimal IndirimliFiyat => IndirimOrani.HasValue && IndirimOrani.Value > 0
            ? Fiyat * (1 - IndirimOrani.Value / 100)
            : Fiyat;

        [NotMapped]
        public bool Indirimli => IndirimOrani.HasValue && IndirimOrani.Value > 0;

        [NotMapped]
        public bool StoktaVar => Stok > 0;

        [NotMapped]
        public bool StokKritik => Stok <= 5 && Stok > 0;

        [NotMapped]
        public bool StokYok => Stok <= 0;

        [NotMapped]
        public string StokDurumu => StokYok ? "Stokta Yok" : StokKritik ? "Kritik Stok" : "Stokta Var";

        [NotMapped]
        public double OrtalamaPuan => Yorumlar?.Where(y => y.Onaylandi).Any() == true
            ? Yorumlar.Where(y => y.Onaylandi).Average(y => y.Puan)
            : (double) (VarsayilanPuan ?? 0);

        [NotMapped]
        public int YorumSayisi => Yorumlar?.Where(y => y.Onaylandi).Count() ?? 0;

        [NotMapped]
        public List<string> EtiketListesi => !string.IsNullOrEmpty(Etiketler)
            ? Etiketler.Split(',').Select(e => e.Trim()).ToList()
            : new List<string>();

        [NotMapped]
        public string KisaAciklama => !string.IsNullOrEmpty(Aciklama) && Aciklama.Length > 100
            ? Aciklama.Substring(0, 100) + "..."
            : Aciklama;

        public void StokDusur(int adet)
        {
            if (Stok >= adet)
            {
                Stok -= adet;
                GuncellemeTarihi = DateTime.Now;
            }
            else
            {
                throw new InvalidOperationException($"Yeterli stok yok. Mevcut: {Stok}, İstenen: {adet}");
            }
        }

        public void StokEkle(int adet)
        {
            if (adet > 0)
            {
                Stok += adet;
                GuncellemeTarihi = DateTime.Now;
            }
        }

        public void IndirimUygula(decimal oran)
        {
            if (oran >= 0 && oran <= 100)
            {
                IndirimOrani = oran;
                GuncellemeTarihi = DateTime.Now;
            }
            else
            {
                throw new ArgumentException("İndirim oranı 0-100 arasında olmalıdır.");
            }
        }

        public void IndirimKaldir()
        {
            IndirimOrani = null;
            GuncellemeTarihi = DateTime.Now;
        }

        public void EtiketEkle(string etiket)
        {
            if (!string.IsNullOrWhiteSpace(etiket))
            {
                var etiketler = EtiketListesi;
                if (!etiketler.Contains(etiket, StringComparer.OrdinalIgnoreCase))
                {
                    etiketler.Add(etiket);
                    Etiketler = string.Join(",", etiketler);
                    GuncellemeTarihi = DateTime.Now;
                }
            }
        }

        public void EtiketKaldir(string etiket)
        {
            if (!string.IsNullOrWhiteSpace(etiket))
            {
                var etiketler = EtiketListesi;
                etiketler.RemoveAll(e => e.Equals(etiket, StringComparison.OrdinalIgnoreCase));
                Etiketler = string.Join(",", etiketler);
                GuncellemeTarihi = DateTime.Now;
            }
        }

        public bool EtiketVarMi(string etiket)
        {
            return EtiketListesi.Any(e => e.Equals(etiket, StringComparison.OrdinalIgnoreCase));
        }

        public void AktifYap()
        {
            Aktif = true;
            GuncellemeTarihi = DateTime.Now;
        }

        public void PasifYap()
        {
            Aktif = false;
            GuncellemeTarihi = DateTime.Now;
        }

        public void Guncelle(string ad, string aciklama, decimal fiyat, int stok, int kategoriId, string? resimUrl = null)
        {
            Ad = ad;
            Aciklama = aciklama;
            Fiyat = fiyat;
            Stok = stok;
            KategoriId = kategoriId;
            ResimUrl = resimUrl;
            GuncellemeTarihi = DateTime.Now;
        }
    }
}