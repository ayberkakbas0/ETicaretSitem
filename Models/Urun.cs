using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaretSitesi.Models
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
    }
} 