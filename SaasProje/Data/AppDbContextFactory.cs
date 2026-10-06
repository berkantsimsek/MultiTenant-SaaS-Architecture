using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSProje.Services;

namespace SaaSProje.Data
{
    
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SaasDb;Trusted_Connection=True;MultipleActiveResultSets=true");

            
            return new AppDbContext(optionsBuilder.Options, new DummyTenantService());
        }
    }


    class DummyTenantService : ICurrentTenantService
    {
        public Guid? FirmId => null;
        public void SetFirmId(Guid firmId) { }
    }
}