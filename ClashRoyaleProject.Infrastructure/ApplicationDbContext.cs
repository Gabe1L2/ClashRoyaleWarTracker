using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClashRoyaleProject.Application.Models;

namespace ClashRoyaleProject.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Clan> Clans { get; set; } = null!;
        public DbSet<ClanHistory> ClanHistories { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<PlayerAverage> PlayerAverages { get; set; } = null!;
        public DbSet<RawWarData> RawWarData { get; set; } = null!;
        public DbSet<WarData> WarData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index for unique Tags
            modelBuilder.Entity<Clan>()
                .HasIndex(c => c.Tag)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Tag)
                .IsUnique();

            // Foreign Keys
            modelBuilder.Entity<ClanHistory>()
                .HasOne<Clan>()
                .WithMany()
                .HasForeignKey(ch => ch.ClanID)
                .HasPrincipalKey(c => c.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerAverage>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(pa => pa.PlayerID)
                .HasPrincipalKey(p => p.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerAverage>()
                .HasOne<Clan>()
                .WithMany()
                .HasForeignKey(pa => pa.ClanID)
                .HasPrincipalKey(c => c.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RawWarData>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(rwd => rwd.PlayerID)
                .HasPrincipalKey(p => p.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RawWarData>()
                .HasOne<ClanHistory>()
                .WithMany()
                .HasForeignKey(rwd => rwd.ClanHistoryID)
                .HasPrincipalKey(ch => ch.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WarData>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(wd => wd.PlayerID)
                .HasPrincipalKey(p => p.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WarData>()
                .HasOne<ClanHistory>()
                .WithMany()
                .HasForeignKey(wd => wd.ClanHistoryID)
                .HasPrincipalKey(ch => ch.ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate clan history entries for same clan/season/week
            modelBuilder.Entity<ClanHistory>()
                .HasIndex(ch => new { ch.ClanID, ch.SeasonID, ch.WeekIndex })
                .IsUnique();

            // Prevent duplicate player averages for same player/5k or 4k
            modelBuilder.Entity<PlayerAverage>()
                .HasIndex(pa => new { pa.PlayerID, pa.Is5k })
                .IsUnique();

            // Prevent duplicate war data for same player
            modelBuilder.Entity<WarData>()
                .HasIndex(wd => new { wd.PlayerID, wd.ClanHistoryID })
                .IsUnique();

            // Prevent duplicate raw war data entries (same player/clan history)
            modelBuilder.Entity<RawWarData>()
                .HasIndex(rwd => new { rwd.PlayerID, rwd.ClanHistoryID })
                .IsUnique();
        }
    }
}
