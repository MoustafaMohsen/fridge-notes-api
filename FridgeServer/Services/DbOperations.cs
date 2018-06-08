using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FridgeServer.Data;
using FridgeServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FridgeServer.Services
{
    public class DbOperations
    {
        private AppDbContext db;
        public DbOperations(AppDbContext _db)
        {
            db = _db;
        }


        public async  void  TestOperations()
        {
            //get data from db with matching queries
            List<Grocery> DataList = await db.Grocery.Where(G => G.basic).Select(G => new Grocery
            {
                Id = G.Id,
                Timeout = G.Timeout
            }).ToListAsync();

            if (DataList.Count == 0)
            {
                return;
            }
            //prefore a check in each item in DataList
            foreach (var item in DataList)
            {
                Double now = Alarm.DateTimeToUnixTime(DateTime.Now);

                if (item.Timeout < now)
                {
                    //Timeout Due Logic Here
                    var grocery = await db.Grocery.SingleOrDefaultAsync(m => m.Id == item.Id);
                    if (grocery == null)
                    {
                        return;
                    }
                    // Console.WriteLine(grocery);
                    // db.Grocery.Remove(grocery);
                    // await db.SaveChangesAsync();
                     Alarm.TestVariable.Add(grocery);
                }
            }

        }


    }
}
