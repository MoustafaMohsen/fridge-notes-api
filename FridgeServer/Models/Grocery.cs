using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using FridgeServer.Services;
using Microsoft.EntityFrameworkCore;

namespace FridgeServer.Models
{

    public class Grocery
    {
        [Key]
        public string id { get; set; }

        [MaxLength(length: 128)]
        [Required]
        public string name { get; set; }


        public List<MoreInformation> moreInformations { get; set; } = new List<MoreInformation>() { };

        [Required]
        public bool basic { get; set; } = false;

        [Required]
        public bool groceryOrBought { get; set; } = false; // false = Need / true=Bought

        [MaxLength(length: 265)]
        [Required]
        public string ownerid { get; set; }

        [MaxLength(length: 265)]
        public string owner { get; set; }

        [MaxLength(length: 265)]
        public string excludeids { get; set; }

        
        public long? timeout { get; set; }//current Lifetime of the item

        [MaxLength(length: 265)]
        public string category { get; set; } = "";

        [MaxLength(length: 512)]
        public string style { get; set; } = "";

        // Foreign Key
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
    }


    public class MoreInformation
    {
        [Key]
        public string id { get; set; }
        public long? date { get; set; }// = (long)Alarm.DateTimeToUnixTime(DateTime.Now);  //the date in which the item have been bought or added/Needed to note
        public bool bought { get; set; } = false; // false = Need / true=Bought
        public long? lifeTime { get; set; } = 0;//the lifetime number
        //Details
        public int? no { get; set; } = 1;

        [MaxLength(length: 64)]
        public string typeOfNo { get; set; }

        //Foreign Key
        [ForeignKey("Grocery")]
        public int Groceryid { get; set; }
    }

}
