using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Data;
using FridgeServer.Models;

namespace FridgeServer.Helpers
{
    public abstract class M
    {
        //===================Helper methods============//
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
        public static int StringToInt(string str)
        {
            int x = 0;
            if (str == null)
            {
                return x;
            }
            for (System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(str, @"\d+"); match.Success; match = match.NextMatch())
            {
                x = int.Parse(match.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            return x;
        }
        public static string StringfyObject<T>(T Object)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Object);
        }
        public static bool isNull<T>(T Object)
        {
            if (Object == null)
            {
                return true;
            }
            return false;
        }
        public static bool isNullOr0<T>(T Object)
        {
            if (Object == null || Object.Equals(0))
            {
                return true;
            }
            return false;
        }

        private static Random random = new Random();
        public static string RandomString(int length,string ProvidedChars="")
        {
            const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string chars = ProvidedChars.Length==0? _chars : ProvidedChars;
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }//class
}