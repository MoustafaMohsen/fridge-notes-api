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
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<MoreInformations> MoreInformations { get; set; } = new List<MoreInformations>() { };
        [Required]
        public bool basic { get; set; } = false;
        //basic only
        public long? Timeout { get; set; }//current Lifetime of the item

        public bool groceryOrBought { get; set; } = false; // false = Need / true=Bought

    }


    public class MoreInformations
    {
        public int MoreInformationsId { get; set; }
        public long? Date { get; set; }// = (long)Alarm.DateTimeToUnixTime(DateTime.Now);  //the date in which the item have been bought or added/Needed to note
        public bool Bought { get; set; } = false; // false = Need / true=Bought
        public long? LifeTime { get; set; } = 0;//the lifetime number
        //Details
        public int? No { get; set; } = 1;
        public string typeOfNo { get; set; }
    }


}
