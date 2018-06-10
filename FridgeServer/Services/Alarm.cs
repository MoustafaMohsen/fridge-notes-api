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

    }//class
}