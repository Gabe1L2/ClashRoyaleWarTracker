using ClashRoyaleWarTracker.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ClashRoyaleWarTracker.Application.Interfaces;

namespace ClashRoyaleWarTracker.Infrastructure.Repositories
{
    public class PlayerRepository
    {
        private readonly ApplicationDbContext _context;
        public PlayerRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}