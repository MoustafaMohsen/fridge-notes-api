using FridgeServer.Data;
using FridgeServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace FridgeServer.Services
{
    public class MyService //: IMyService
    {
        private readonly AppDbContext _context;

        public static string CanUHearMe = "Yes I can" ;
        public static int i=1;
        public MyService(AppDbContext ctx)
        {
            _context = ctx;
            test();
        }
        
        public List<Grocery> GetUser()
        {
            var users = from u in _context.Grocery where u.basic == true select u;

     
                return users.ToList();
 
        }
        public  void test()
        {
            CanUHearMe += " Called : "+ i;
            i++;
        }


    }



    public interface IMyService
    {
        List<Grocery> GetUser();
    }



}



    

