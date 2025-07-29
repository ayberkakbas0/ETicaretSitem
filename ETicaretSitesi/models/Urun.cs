using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Urun
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        [Required]
        public decimal Fiyat { get; set; }
        [Required]
        public int Stok { get; set; }
        [Required]
        public int KategoriId { get; set; }
        public string? ResimUrl { get; set; }

        [ForeignKey("KategoriId")]
        public Kategori Kategori { get; set; }

        
        public List<Yorum> Yorumlar { get; set; } = new List<Yorum>();

       
        public double OrtalamaPuan => Yorumlar?.Where(y => y.Onaylandi).Any() == true
            ? Yorumlar.Where(y => y.Onaylandi).Average(y => y.Puan)
            : 0;
    }
}