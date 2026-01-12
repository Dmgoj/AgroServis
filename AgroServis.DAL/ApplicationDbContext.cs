using AgroServis.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.DAL
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<Maintenance> MaintenanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Maintenance>().HasQueryFilter(m => m.DeletedAt == null);
        }
    }
}