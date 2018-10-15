using Microsoft.EntityFrameworkCore;
using FridgeServer.Models;

namespace FridgeServer.Data
{
    public class AppDbContext : DbContext
    {

        private string connectionString;

        //Scoped constructor
        public AppDbContext(DbContextOptions<AppDbContext>  options) : base(options)
        {
        }
        
        //Singletone constructor
        public AppDbContext(DbContextOptions<AppDbContext> options,string connection)
        {
            connectionString = connection;
        }
        


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Used when initializing db context outside IoC (inversion of control)
            if (connectionString != null)
            {
                var config = connectionString;
                optionsBuilder.UseSqlite(config);
            }

            base.OnConfiguring(optionsBuilder);
        }

        
        //DbSet

        public DbSet<User> Users { get; set; }
        public DbSet<Grocery> Grocery { get; set; }
        public DbSet<MoreInformations> MoreInformations { get; set; }

    }//class
}
