using Microsoft.EntityFrameworkCore;
using ecopack.Api.Dtos;

namespace ecopack.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ★ Product 테이블 통로 추가
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ecouser> Ecousers => Set<Ecouser>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<If001> If001 => Set<If001>();
        public DbSet<If002> If002 => Set<If002>();
        public DbSet<If002a> If002a => Set<If002a>();
        public DbSet<If003> If003 => Set<If003>();
        public DbSet<If003a> If003a => Set<If003a>();
        public DbSet<If004> If004 => Set<If004>();
        public DbSet<If005> If005 => Set<If005>();
        public DbSet<If100> If100 => Set<If100>();
        public DbSet<PrimaryDoc> PrimaryDoc => Set<PrimaryDoc>();
        public DbSet<PrimaryPkg> PrimaryPkg => Set<PrimaryPkg>();
        public DbSet<PrimaryTd> PrimaryTd => Set<PrimaryTd>();
        public DbSet<Project> Project => Set<Project>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}