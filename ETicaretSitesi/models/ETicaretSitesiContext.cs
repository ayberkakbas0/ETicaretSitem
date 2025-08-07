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
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Yorum> Yorumlar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
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

                
                entity.HasOne(e => e.Urun)
                    .WithMany()
                    .HasForeignKey(e => e.UrunId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Kullanici)
                    .WithMany()
                    .HasForeignKey(e => e.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasIndex(e => new { e.KullaniciId, e.UrunId })
                    .IsUnique();

                
                entity.HasIndex(e => e.UrunId);
                entity.HasIndex(e => e.KullaniciId);
                entity.HasIndex(e => e.Tarih);
                entity.HasIndex(e => e.Onaylandi);
            });

            
            modelBuilder.Entity<Urun>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Ad)
                    .IsRequired();

                entity.Property(e => e.Fiyat)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Stok)
                    .IsRequired();

                
                entity.HasOne(e => e.Kategori)
                    .WithMany()
                    .HasForeignKey(e => e.KategoriId)
                    .OnDelete(DeleteBehavior.Restrict);

                
                entity.HasMany(e => e.Yorumlar)
                    .WithOne(y => y.Urun)
                    .HasForeignKey(y => y.UrunId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            
            modelBuilder.Entity<Kategori>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Isim)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            
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

               
                entity.HasIndex(e => e.Telefon)
                    .IsUnique()
                    .HasFilter("[Telefon] IS NOT NULL");

                entity.Property(e => e.KayitTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.AdminMi)
                    .IsRequired()
                    .HasDefaultValue(false);

                
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                
                entity.HasMany(e => e.Yorumlar)
                    .WithOne(y => y.Kullanici)
                    .HasForeignKey(y => y.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            
            modelBuilder.Entity<Siparis>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SiparisTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ToplamTutar)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Durum)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Adres)
                    .IsRequired();

                entity.Property(e => e.OdemeDurumu)
                    .HasMaxLength(20);

                entity.Property(e => e.OdemeYontemi)
                    .HasMaxLength(50);

                entity.Property(e => e.TakipKodu)
                    .HasMaxLength(50);

                entity.Property(e => e.Notlar)
                    .HasMaxLength(500);

                
                entity.HasOne(e => e.Kullanici)
                    .WithMany()
                    .HasForeignKey(e => e.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.OnaylayanAdmin)
                    .WithMany()
                    .HasForeignKey(e => e.OnaylayanAdminId)
                    .OnDelete(DeleteBehavior.Restrict);



                
                entity.HasIndex(e => e.KullaniciId);
                entity.HasIndex(e => e.SiparisTarihi);
                entity.HasIndex(e => e.Durum);
                entity.HasIndex(e => e.OdemeDurumu);
            });



            
            modelBuilder.Entity<Odeme>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OdemeYontemi)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Tutar)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Durum)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.OdemeTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.IslemKodu)
                    .HasMaxLength(50);

                entity.Property(e => e.KartNumarasi)
                    .HasMaxLength(20);

                entity.Property(e => e.Aciklama)
                    .HasMaxLength(500);

                
                entity.HasOne(e => e.Siparis)
                    .WithMany()
                    .HasForeignKey(e => e.SiparisId)
                    .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasIndex(e => e.SiparisId);
                entity.HasIndex(e => e.OdemeTarihi);
                entity.HasIndex(e => e.Durum);
            });
        }
    }
}