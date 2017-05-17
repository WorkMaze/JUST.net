using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JUST.NET.Test
{
    public class Season
    {
        public static string findseason(string leafColour, string flowerColour)
        {
            if (leafColour == "green" && flowerColour == "red")
                return "summer";
            else
                return "winter";
        }
    }
}
