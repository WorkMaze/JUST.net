using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SeasonsHelper
{
    public class Season
    {
        public static bool IsSummer(bool hot, int averageDaysOfRain, DateTime randomDay)
        {
            return hot && averageDaysOfRain < 20 && randomDay > new DateTime(2018, 6, 21);
        }

        public static string findseason(string leafColour, string flowerColour)
        {
            if (leafColour == "green" && flowerColour == "red")
                return "summer";
            else
                return "winter";
        }

        public static object serialize(string inObj)
        {
            JObject jObj = JObject.FromObject(JsonConvert.DeserializeObject(inObj));

            JArray array = new JArray();

            foreach (JProperty property in jObj.Children())
            {
                JObject nameValObj = new JObject();
                nameValObj.Add("name", property.Name);
                nameValObj.Add("value", property.Value);

                array.Add(nameValObj);
            }

            JObject returnObj = new JObject();
            returnObj.Add("namevaluecollection", array);

            return returnObj;
        }
    }
}
