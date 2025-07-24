using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaretSitesi.Models
{
    public class Siparis
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int KullaniciId { get; set; }
        [Required]
        public decimal ToplamTutar { get; set; }
        public DateTime SiparisTarihi { get; set; } = DateTime.Now;
        [Required]
        public int SiparisDurumuId { get; set; }

        [ForeignKey("KullaniciId")]
        public Kullanici Kullanici { get; set; }
        [ForeignKey("SiparisDurumuId")]
        public SiparisDurumu SiparisDurumu { get; set; }
    }
} 