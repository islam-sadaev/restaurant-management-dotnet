using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data
{
    public class RestaurantContext : IdentityDbContext<CustomUser>
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Land> Landen { get; set; }
        public DbSet<CustomUser> CustomUsers { get; set; }
        public DbSet<Status> Statussen { get; set; }
        public DbSet<Tijdslot> Tijdslots { get; set; }
        public DbSet<Reservatie> Reservaties { get; set; }
        public DbSet<TafelLijst> TafelLijsten { get; set; }
        public DbSet<Tafel> Tafels { get; set; }
        public DbSet<Sluitingsdag> Sluitingsdagen { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<CategorieType> Types { get; set; }
        public DbSet<Categorie> Categorien { get; set; }
        public DbSet<Product> Producten { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<PrijsProduct> PrijsProducten { get; set; }
        public DbSet<Bestelling> Bestellingen { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints

            // Land
            modelBuilder.Entity<Land>(entity =>
            {
                entity.ToTable("Land");
            });

            // Status
            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");
            });

            // TijdSlot
            modelBuilder.Entity<Tijdslot>(entity =>
            {
                entity.ToTable("TijdSlot");
            });

            // Reservatie
            modelBuilder.Entity<Reservatie>(entity =>
            {
                entity.ToTable("Reservatie");

                // Hier wordt de CustomUserId fout opgelost:
                entity.HasOne(r => r.CustomUser)
                    .WithMany(cu => cu.Reservaties)
                    .HasForeignKey(r => r.KlantId) // Koppelt KlantId aan CustomUser
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Tijdslot)
                    .WithMany(ts => ts.Reservaties)
                    .HasForeignKey(r => r.TijdSlotId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TafelLijst
            modelBuilder.Entity<TafelLijst>(entity =>
            {
                entity.ToTable("TafelLijst");

                entity.HasOne(tl => tl.Reservatie)
                    .WithMany(t => t.Tafellijsten)
                    .HasForeignKey(tl => tl.ReservatieId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tl => tl.Tafel)
                    .WithMany(t => t.TafelLijsten)
                    .HasForeignKey(tl => tl.TafelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Tafel
            modelBuilder.Entity<Tafel>(entity =>
            {
                entity.ToTable("Tafel");
            });

            // Sluitingsdag
            modelBuilder.Entity<Sluitingsdag>(entity =>
            {
                entity.ToTable("Sluitingsdag");
            });

            // Mail
            modelBuilder.Entity<Mail>(entity =>
            {
                entity.ToTable("Mail");
            });

            // Type
            modelBuilder.Entity<CategorieType>(entity =>
            {
                entity.ToTable("CategorieType");
            });

            // Categorie
            modelBuilder.Entity<Categorie>(entity =>
            {
                entity.ToTable("Categorie");

                entity.HasOne(c => c.Type)
                    .WithMany(t => t.Categorieen)
                    .HasForeignKey(c => c.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.HasOne(p => p.Categorie)
                    .WithMany(c => c.Producten)
                    .HasForeignKey(p => p.CategorieId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Parameter
            modelBuilder.Entity<Parameter>(entity =>
            {
                entity.ToTable("Parameter");
            });

            // PrijsProduct
            modelBuilder.Entity<PrijsProduct>(entity =>
            {
                entity.ToTable("PrijsProduct");

                entity.HasOne(pp => pp.Product)
                    .WithMany(p => p.PrijsProducten)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bestelling
            modelBuilder.Entity<Bestelling>(entity =>
            {
                entity.ToTable("Bestelling");

                // Hier worden de ProductId1 en StatusId1 fouten opgelost
                // Door expliciet de ForeignKeys te noemen en dubbele code te verwijderen.

                entity.HasOne(b => b.Reservatie)
                    .WithMany(r => r.Bestellingen)
                    .HasForeignKey(b => b.ReservatieId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Product)
                    .WithMany(p => p.Bestellingen)
                    .HasForeignKey(b => b.ProductId) // Koppelt ProductId aan Product
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Status)
                    .WithMany(s => s.Bestellingen)
                    .HasForeignKey(b => b.StatusId) // Koppelt StatusId aan Status
                    .OnDelete(DeleteBehavior.Restrict);
            });

            
        }
    }
}