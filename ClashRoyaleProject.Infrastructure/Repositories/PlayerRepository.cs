using ClashRoyaleProject.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ClashRoyaleProject.Application.Interfaces;

namespace ClashRoyaleProject.Infrastructure.Repositories
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