using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Data;
using FridgeServer.Models;

namespace FridgeServer.Services
{
    public class Alarm
    {

        public Alarm(AppDbContext _db)
        {
            db = _db;
        }


        private static Timer aTimer;
        private  AppDbContext db;

        public  void StartTimerEvent()
        {
            DateTime now = DateTime.Now;
            // Create a timer and set a two second interval.
            aTimer = new Timer();
            aTimer.Interval = 6000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += (source ,e)=> CheckDBEvents( source,e );
            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;
            // Start the timer
            aTimer.Enabled = true;

        }


        private  void CheckDBEvents(Object source, ElapsedEventArgs e)
        {
            
            //get data from db with matching queries
            List<Grocery> DataList =  db.Grocery.Where(G => G.basic).Select(G => new Grocery
            {
                Id = G.Id,
                Timeout = G.Timeout
            }).ToList();

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
                    var grocery =  db.Grocery.SingleOrDefault(m => m.Id == item.Id);
                    if (grocery == null)
                    {
                        return;
                    }
                    // Console.WriteLine(grocery);
                    // db.Grocery.Remove(grocery);
                    // await db.SaveChangesAsync();
                    TestVariable.Add(grocery);
                }
            }
         }




        //===================Helper methods============//
        //Variable Passer
        public static List<Grocery> TestVariable;
        //Time Convertors Logic
        public static DateTime UnixTimeToDateTime(double UnixTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(UnixTime);
        }
        public static double DateTimeToUnixTime(DateTime DateTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }



    }
}

//Old Code
/*
//get data from db and save it to DataList
private static List<Grocery> DataList;
public static void GetAll(AppDbContext db)
{
    DataList = db.Grocery.Where(G => G.basic).Select(G => new Grocery
    {
        Id = G.Id,
        Timeout = G.Timeout
    }).ToList();
} 
 */
