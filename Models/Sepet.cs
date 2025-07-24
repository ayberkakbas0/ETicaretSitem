using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaretSitesi.Models
{
    public class Sepet
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int KullaniciId { get; set; }
        [Required]
        public int UrunId { get; set; }
        [Required]
        public int Adet { get; set; }
        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

        [ForeignKey("KullaniciId")]
        public Kullanici Kullanici { get; set; }
        [ForeignKey("UrunId")]
        public Urun Urun { get; set; }
    }
} 