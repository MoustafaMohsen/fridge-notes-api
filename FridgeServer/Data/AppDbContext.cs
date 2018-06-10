using Microsoft.EntityFrameworkCore;
using FridgeServer.Models;

namespace FridgeServer.Data
{
    public class AppDbContext : DbContext
    {
        //Scoped constructor
        public AppDbContext(DbContextOptions<AppDbContext>  options) : base(options)
        {
        }

        //Singletone constructor
        public AppDbContext(DbContextOptions<AppDbContext> options,string connection)
        {
            connectionString = connection;
        }
        private string connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Used when initializing db context outside IoC (inversion of control)
            if (connectionString != null)
            {
                var config = connectionString;
                optionsBuilder.UseSqlServer(config);
            }

            base.OnConfiguring(optionsBuilder);
        }
        //DbSet

        public DbSet<Grocery> Grocery { get; set; }
        public DbSet<MoreInformations> MoreInformations { get; set; }

    }
}
