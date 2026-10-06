using Microsoft.EntityFrameworkCore;
using SaaSProje.Models;
using SaaSProje.Services;

namespace SaaSProje.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentTenantService _currentTenantService;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService currentTenantService)
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        // Tablolarımız
        public DbSet<Firm> Firms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }   
        public DbSet<Order> Orders { get; set; } 
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasQueryFilter(u => u.FirmId == _currentTenantService.FirmId);
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.FirmId == _currentTenantService.FirmId);

            // Yeni tabloların başkasının verisini görmesini engelleyen izolasyon kuralları
            modelBuilder.Entity<Cart>().HasQueryFilter(c => c.FirmId == _currentTenantService.FirmId);
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.FirmId == _currentTenantService.FirmId);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => oi.FirmId == _currentTenantService.FirmId);
        }

        public override int SaveChanges()
        {
            SetTenantId();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SetTenantId();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantId()
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().Where(e => e.State == EntityState.Added))
            {
                if (_currentTenantService.FirmId.HasValue)
                {
                    entry.Entity.FirmId = _currentTenantService.FirmId.Value;
                }
            }
        }
    }
}