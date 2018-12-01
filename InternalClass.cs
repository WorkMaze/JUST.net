using Newtonsoft.Json.Linq;
using System;

namespace InternalMethods
{
    public class InternalClass
    {
        public static string StaticMethod()
        {
            return "Internal Static";
        }

        public string InstanceMethod()
        {
            return "Internal Instance";
        }

        public string StaticTypedParameters(int n, bool b, string s, DateTime d)
        {
            return "Internal Static TypedParameters success";
        }

        public string TypedParameters(int n, bool b, string s, DateTime d)
        {
            return "Internal TypedParameters success";
        }

        public string NavigateTypedParameters(JObject token)
        {
            return token.ToString();
        }
    }
}
