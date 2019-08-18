using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static object findseasontemperaturetable(object data)
        {
            var temperatures = JArray.Parse(JsonConvert.SerializeObject(data));
            var seasons = new string[] { "summer", "spring", "fall", "winter" };
            var years = new string[] { "2017", "2018", "2019" };
            var dataIn = JArray.Parse(JsonConvert.SerializeObject(data));
            var result = new List<List<string[]>>();
            foreach (string year in years)
            {
                var resultRow = new List<string[]> { new string[] { year } };
                foreach (string season in seasons)
                {
                    var current = temperatures.Where(x => x["year"].ToString() == year && x["season"].ToString() == season).Select(x => x["temperature"].Value<decimal>()).Average();
                    resultRow.Add(new string[] { current.ToString() });
                }
                result.Add(resultRow);
            }
            return result;
        }
    }
}
