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

        public string NavigateTypedNullParameters(object val)
        {
            return val?.ToString();
        }

        public object CheckNullParameters(object val1, object val2)
        {
            return val1 ?? val2;
        }
    }
}
