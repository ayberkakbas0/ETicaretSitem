using Microsoft.EntityFrameworkCore;
namespace ETicaretSitesi.models
{
public class ETicaretSitesiContext : DbContext
{
    public ETicaretSitesiContext(DbContextOptions<ETicaretSitesiContext> options)
        : base(options)
    {
    }

    public DbSet<Kategori> Kategori { get; set; }
    public DbSet<Kullanici> Kullanici { get; set; }
    public DbSet<Sepet> Sepet { get; set; }
    public DbSet<Siparis> Siparis { get; set; }
    public DbSet<SiparisDurumu> SiparisDurumu { get; set; }
    public DbSet<Urun> Urunler { get; set; }
}
}