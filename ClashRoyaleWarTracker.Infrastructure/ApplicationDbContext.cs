using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClashRoyaleWarTracker.Application.Models;

namespace ClashRoyaleWarTracker.Infrastructure
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
        public DbSet<PlayerWarHistory> PlayerWarHistories { get; set; } = null!;

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

            modelBuilder.Entity<Player>()
                .HasOne<Clan>()
                .WithMany()
                .HasForeignKey(p => p.ClanID)
                .HasPrincipalKey(c => c.ID)
                .OnDelete(DeleteBehavior.SetNull);

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
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PlayerWarHistory>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(pwh => pwh.PlayerID)
                .HasPrincipalKey(p => p.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerWarHistory>()
                .HasOne<ClanHistory>()
                .WithMany()
                .HasForeignKey(pwh => pwh.ClanHistoryID)
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

            // Prevent duplicate war data for same player/clan history entry
            modelBuilder.Entity<PlayerWarHistory>()
                .HasIndex(pwh => new { pwh.PlayerID, pwh.ClanHistoryID })
                .IsUnique();
        }
    }
}
