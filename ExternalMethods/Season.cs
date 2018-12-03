using System;

namespace SeasonsHelper
{
    public class Season
    {
        public static bool IsSummer(bool hot, int averageDaysOfRain, DateTime randomDay)
        {
            return hot && averageDaysOfRain < 20 && randomDay > new DateTime(2018, 6, 21);
        }
    }
}
