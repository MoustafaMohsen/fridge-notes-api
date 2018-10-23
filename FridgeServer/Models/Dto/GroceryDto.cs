using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class GroceryDto
    {
        public Grocery grocery { get; set; }
        public int userId { get; set; }
    }
}
