using CitiesManager.WebAPI.model;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.WebAPI.DataBaseContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public ApplicationDbContext()
        {
            
        }

        public DbSet<City> Cities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<City>().HasData(new List<City>() {
                new City()
                { 
                    CityId=Guid.Parse("3ace8bb6-9464-417d-b3c6-cc74d1afb627"),
                    CityName="NewYork"
                },
                new City() 
                {
                    CityId=Guid.Parse("762f70ef-9fbb-41dc-b370-287b5f376278"),
                    CityName="London"
                },
            });
        }
    }
}
