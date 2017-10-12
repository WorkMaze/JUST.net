using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static object serialize(string inObj)
        {
            JObject jObj = JObject.FromObject( JsonConvert.DeserializeObject(inObj));

            JArray array = new JArray();

            foreach(JProperty property in jObj.Children())
            {
                JObject nameValObj = new JObject();
                nameValObj.Add("name", property.Name);
                nameValObj.Add("value", property.Value);

                array.Add(nameValObj);
            }

            JObject returnObj = new JObject();
            returnObj.Add("namevaluecollection",array);

            return returnObj;
        }
    }
}
