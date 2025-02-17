using LaundryApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace LaundryApplication.Data
{
    public class LaundryDbContext : DbContext
    {
        public LaundryDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
