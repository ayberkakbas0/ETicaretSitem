using System.ComponentModel.DataAnnotations;

namespace ETicaretSitesi.Models
{
    public class Kullanici
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Ad { get; set; }
        [Required]
        public string Soyad { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Sifre { get; set; }
        [Required]
        public string Rol { get; set; } // "admin" veya "kullanici"
    }
} 