using Newtonsoft.Json.Linq;
using System;

namespace ExternalMethods
{
    public class ExternalClass
    {
        public static string StaticMethod()
        {
            return "External Static";
        }

        public string InstanceMethod()
        {
            return "External Instance";
        }

        public static string StaticTypedParameters(int n, bool b, string s, DateTime d)
        {
            return "External Static TypedParameters success";
        }

        public string TypedParameters(int n, bool b, string s, DateTime d)
        {
            return "External TypedParameters success";
        }

        public string NavigateTypedParameters(bool val)
        {
            return val.ToString();
        }
    }
}
