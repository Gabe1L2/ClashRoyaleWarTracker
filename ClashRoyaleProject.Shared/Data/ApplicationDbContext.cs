using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClashRoyaleProject.Shared.Models;

namespace ClashRoyaleProject.Shared.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Clan> Clans { get; set; } = null!;
        public DbSet<ClanHistory> ClanHistories { get; set; } = null!;
        public DbSet<RawWarData> RawWarData { get; set; } = null!;
        public DbSet<WarData> WarData { get; set; } = null!;
        public DbSet<PlayerAverage> PlayerAverages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<ClanHistory>()
                .HasOne<Clan>()
                .WithMany(c => c.Histories)
                .HasForeignKey(ch => ch.ClanID);

            // Configure unique constraints
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Tag)
                .IsUnique();

            modelBuilder.Entity<Clan>()
                .HasIndex(c => c.Tag)
                .IsUnique();
        }
    }
}