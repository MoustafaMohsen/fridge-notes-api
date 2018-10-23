using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FridgeServer.Services;
using Microsoft.EntityFrameworkCore;

namespace FridgeServer.Models
{
    public class Grocery
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        public List<MoreInformation> moreInformations { get; set; } = new List<MoreInformation>() { };
        [Required]
        public bool basic { get; set; } = false;
        //basic only
        public long? timeout { get; set; }//current Lifetime of the item

        public bool groceryOrBought { get; set; } = false; // false = Need / true=Bought
        public string owner { get; set; }

        public int Userid { get; set; }
    }


    public class MoreInformation
    {
        public int id { get; set; }
        public long? date { get; set; }// = (long)Alarm.DateTimeToUnixTime(DateTime.Now);  //the date in which the item have been bought or added/Needed to note
        public bool bought { get; set; } = false; // false = Need / true=Bought
        public long? lifeTime { get; set; } = 0;//the lifetime number
        //Details
        public int? no { get; set; } = 1;
        public string typeOfNo { get; set; }

        public int Groceryid { get; set; }
    }



    public class Sport
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<League> leagues { get; set; }
    }
    public class League
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Team> teams { get; set; }

        public int Sportid { get; set; }
    }
    public class Team
    {
        public int id { get; set; }
        public string name { get; set; }
        public string successrate { get; set; }
        public List<Player> players { get; set; }

        public int Leagueid { get; set; }
    }
    public class Player
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }

        public int Teamid { get; set; }
    }
}
