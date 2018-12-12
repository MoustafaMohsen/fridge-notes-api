using Microsoft.EntityFrameworkCore;
using FridgeServer.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FridgeServer._UserIdentity;

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
