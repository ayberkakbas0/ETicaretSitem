using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class Kategori
    {
        public int Id { get; set; }
        public string Isim { get; set; }
        public string? Aciklama { get; set; }
        public int? UstKategoriId { get; set; }
        public Kategori? UstKategori { get; set; }
        public ICollection<Kategori>? AltKategoriler { get; set; }
    }
}
