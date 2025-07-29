using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Yorum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yorum metni boş olamaz")]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir")]
        public string Metin { get; set; }

        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Puan { get; set; }

        public DateTime Tarih { get; set; }
        public bool Onaylandi { get; set; }

      
        public int UrunId { get; set; }
        public Urun Urun { get; set; }

        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }
    }
}