using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.models
{
    public class SiparisDurumu
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string? Aciklama { get; set; }
    }
}