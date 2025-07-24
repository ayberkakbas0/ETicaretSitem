using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.Models
{
    public class SiparisDurumu
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Ad { get; set; }

        public ICollection<Siparis> Siparisler { get; set; }
    }
} 