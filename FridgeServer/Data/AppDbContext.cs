using Microsoft.EntityFrameworkCore;
using FridgeServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FridgeServer.Data
{
    public class AppDbContext :  IdentityDbContext<ApplicationUser>
    {
        //Scoped constructor
        public AppDbContext(DbContextOptions<AppDbContext>  options) : base(options)
        {
        }



        //DbSet
        
        public DbSet<Grocery> userGroceries { get; set; }
        public DbSet<MoreInformation> moreInformations { get; set; }
        public DbSet<UserFriend> userFriends { get; set; }
        
    }//class
}
