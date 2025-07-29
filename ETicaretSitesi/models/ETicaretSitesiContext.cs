using Microsoft.EntityFrameworkCore;
using ETicaretSitesi.models;

namespace ETicaretSitesi.models
{
    public class ETicaretSitesiContext : DbContext
    {
        public ETicaretSitesiContext(DbContextOptions<ETicaretSitesiContext> options) : base(options)
        {
        }

        public DbSet<Kategori> Kategori { get; set; }
        public DbSet<Kullanici> Kullanici { get; set; }
        public DbSet<Sepet> Sepet { get; set; }
        public DbSet<Siparis> Siparis { get; set; }
        public DbSet<SiparisDurumu> SiparisDurumu { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Yorum> Yorumlar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Yorum entity konfigürasyonu
            modelBuilder.Entity<Yorum>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Metin)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Puan)
                    .IsRequired();

                entity.Property(e => e.Tarih)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Onaylandi)
                    .IsRequired()
                    .HasDefaultValue(true);

                // İlişkiler
                entity.HasOne(e => e.Urun)
                    .WithMany()
                    .HasForeignKey(e => e.UrunId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Kullanici)
                    .WithMany()
                    .HasForeignKey(e => e.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint - her kullanıcı bir ürün için sadece bir yorum yapabilir
                entity.HasIndex(e => new { e.KullaniciId, e.UrunId })
                    .IsUnique();

                // İndeksler
                entity.HasIndex(e => e.UrunId);
                entity.HasIndex(e => e.KullaniciId);
                entity.HasIndex(e => e.Tarih);
                entity.HasIndex(e => e.Onaylandi);
            });

            // Urun entity konfigürasyonu
            modelBuilder.Entity<Urun>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Ad)
                    .IsRequired();

                entity.Property(e => e.Fiyat)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Stok)
                    .IsRequired();

                // İlişki
                entity.HasOne(e => e.Kategori)
                    .WithMany()
                    .HasForeignKey(e => e.KategoriId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Yorumlar ilişkisi
                entity.HasMany(e => e.Yorumlar)
                    .WithOne(y => y.Urun)
                    .HasForeignKey(y => y.UrunId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Kategori entity konfigürasyonu
            modelBuilder.Entity<Kategori>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Isim)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // Kullanici entity konfigürasyonu
            modelBuilder.Entity<Kullanici>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Ad)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Soyad)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SifreHash)
                    .IsRequired();

                entity.Property(e => e.Telefon)
                    .HasMaxLength(15);

                // Telefon unique (boş değilse)
                entity.HasIndex(e => e.Telefon)
                    .IsUnique()
                    .HasFilter("[Telefon] IS NOT NULL");

                entity.Property(e => e.KayitTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                // Email unique
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                // Yorumlar ilişkisi
                entity.HasMany(e => e.Yorumlar)
                    .WithOne(y => y.Kullanici)
                    .HasForeignKey(y => y.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}