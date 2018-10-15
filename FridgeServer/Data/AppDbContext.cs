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
        

        
        //DbSet

        public DbSet<User> Users { get; set; }
        public DbSet<Grocery> Grocery { get; set; }
        public DbSet<MoreInformations> MoreInformations { get; set; }

    }//class
}
