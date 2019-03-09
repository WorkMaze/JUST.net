using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JUST
{
    internal class Transformer
    {
        public static object valueof(string jsonPath, JUSTContext context)
        {
            JToken token = context.Input;
            JToken selectedToken = token.SelectToken(jsonPath);
            return GetValue(selectedToken);
        }

        public static bool exists(string jsonPath, JUSTContext context)
        {
            JToken token = context.Input;
            JToken selectedToken = token.SelectToken(jsonPath);

            return selectedToken != null;
        }

        public static bool existsandnotempty(string jsonPath, JUSTContext context)
        {
            JToken token = context.Input;
            JToken selectedToken = token.SelectToken(jsonPath);

            return selectedToken != null && selectedToken.ToString().Trim() != string.Empty;
        }

        public static object ifcondition(object condition, object value, object trueResult, object falseResult, JUSTContext context)
        {
            object output = falseResult;

            if (condition.ToString().ToLower() == value.ToString().ToLower())
                output = trueResult;

            return output;
        }

        #region string functions

        public static string concat(string string1, string string2, JUSTContext context)
        {
            string string2Result = (string2 != null) ? string2 : string.Empty;
            return string1 != null ? string1 + string2Result : string.Empty + string2Result;
        }

        public static string substring(string stringRef, int startIndex, int length, JUSTContext context)
        {
            try
            {
                return stringRef.Substring(startIndex, length);
            }
            catch
            {
                return null;
            }
        }

        public static int firstindexof(string stringRef, string searchString, JUSTContext context)
        {
            return stringRef.IndexOf(searchString, 0);
        }

        public static int lastindexof(string stringRef, string searchString, JUSTContext context)
        {
            return stringRef.LastIndexOf(searchString);
        }

        public static string concatall(string array, JUSTContext context)
        {
            string result = null;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    if (result == null)
                        result = string.Empty;
                    result += token.ToString();
                }
            }

            return result;
        }

        public static string concatallatpath(string array, string jsonPath, JUSTContext context)
        {
            string result = null;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {

                foreach (JToken token in parsedArray.Children())
                {

                    JToken selectedToken = token.SelectToken(jsonPath);

                    if (result == null)
                        result = string.Empty;

                    result += selectedToken.ToString();
                }
            }

            return result;
        }

        #endregion

        #region math functions


        public static object add(decimal num1, decimal num2, JUSTContext context)
        {
            return TypedNumber(num1 + num2);
        }

        public static object subtract(decimal num1, decimal num2, JUSTContext context)
        {
            return TypedNumber(num1 - num2);
        }
        public static object multiply(decimal num1, decimal num2, JUSTContext context)
        {
            return TypedNumber(num1 * num2);
        }
        public static object divide(decimal num1, decimal num2, JUSTContext context)
        {
            return TypedNumber(num1 / num2);
        }
        private static object TypedNumber(decimal number)
        {
            return number * 10 % 10 == 0 ? (object)Convert.ToInt32(number) : number;
        }
        #endregion

        #region aggregate functions
        public static object sum(string array, JUSTContext context)
        {
            JArray parsedArray = JArray.Parse(array);
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    result += Convert.ToDecimal(token.ToString());
                }
            }

            return TypedNumber(result);
        }

        public static object sumatpath(string array, string jsonPath, JUSTContext context)
        {
            decimal result = 0;
            JArray parsedArray = JArray.Parse(array);
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = token.SelectToken(jsonPath);
                    result += Convert.ToDecimal(selectedToken.ToString());
                }
            }

            return TypedNumber(result);
        }

        public static object average(string array, JUSTContext context)
        {
            JArray parsedArray = JArray.Parse(array);
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    result += Convert.ToDecimal(token.ToString());
                }
            }

            return TypedNumber(result / parsedArray.Count);
        }

        public static object averageatpath(string array, string jsonPath, JUSTContext context)
        {
            decimal result = 0;
            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = token.SelectToken(jsonPath);
                    result += Convert.ToDecimal(selectedToken.ToString());
                }
            }

            return TypedNumber(result / parsedArray.Count);
        }

        public static object max(string array, JUSTContext context)
        {
            JArray parsedArray = JArray.Parse(array);
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    decimal thisValue = Convert.ToDecimal(token.ToString());
                    result = Math.Max(result, thisValue);
                }
            }

            return TypedNumber(result);
        }

        public static object maxatpath(string array, string jsonPath, JUSTContext context)
        {
            decimal result = 0;
            JArray parsedArray = JArray.Parse(array);
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = token.SelectToken(jsonPath);
                    decimal thisValue = Convert.ToDecimal(selectedToken.ToString());
                    result = Math.Max(result, thisValue);
                }
            }

            return TypedNumber(result);
        }

        public static object min(string array, JUSTContext context)
        {
            JArray parsedArray = JArray.Parse(array);
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    decimal thisValue = Convert.ToDecimal(token.ToString());
                    result = Math.Min(result, thisValue);
                }
            }

            return TypedNumber(result);
        }

        public static object minatpath(string array, string jsonPath, JUSTContext context)
        {
            decimal result = 0;
            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = token.SelectToken(jsonPath);
                    decimal thisValue = Convert.ToDecimal(selectedToken.ToString());
                    result = Math.Min(result, thisValue);
                }
            }

            return TypedNumber(result);
        }

        public static int arraylength(string array, JUSTContext context)
        {
            JArray parsedArray = JArray.Parse(array);
            return parsedArray.Count;
        }

        #endregion

        #region arraylooping
        public static object currentvalue(JArray array, JToken currentElement)
        {
            return GetValue(currentElement);
        }

        public static int currentindex(JArray array, JToken currentElement)
        {
            return array.IndexOf(currentElement);
        }
        
        public static object lastvalue(JArray array, JToken currentElement)
        {
            return GetValue(array.Last);
        }

        public static int lastindex(JArray array, JToken currentElement)
        {
            return array.Count - 1;
        }

        public static object currentvalueatpath(JArray array, JToken currentElement, string jsonPath)
        {
            JToken selectedToken = currentElement.SelectToken(jsonPath);

            return GetValue(selectedToken);
        }

        public static object lastvalueatpath(JArray array, JToken currentElement, string jsonPath)
        {
            JToken selectedToken = array.Last.SelectToken(jsonPath);

            return GetValue(selectedToken);
        }
        #endregion

        #region Constants

        public static string constant_comma(string none, JUSTContext context)
        {
            return ",";
        }

        public static string constant_hash(string none, JUSTContext context)
        {
            return "#";
        }

        #endregion

        #region Variable parameter functions
        public static string xconcat(object[] list)
        {
            string result = string.Empty;

            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i] != null)
                    result += list[i].ToString();
            }

            return result;
        }

        public static object xadd(object[] list)
        {
            JUSTContext context = list[list.Length - 1] as JUSTContext;
            decimal add = 0;
            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i] != null)
                    add += (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[i], context.EvaluationMode);
            }

            return TypedNumber(add);
        }
        #endregion

        public static object GetValue(JToken selectedToken)
        {
            object output = null;
            if (selectedToken != null)
            {

                if (selectedToken.Type == JTokenType.Date)
                {
                    DateTime value = Convert.ToDateTime(selectedToken.Value<DateTime>());
                    output = value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else
                    output = selectedToken.ToString();

                if (selectedToken.Type == JTokenType.Object)
                {
                    output = JsonConvert.SerializeObject(selectedToken);
                }
                if (selectedToken.Type == JTokenType.Boolean)
                {
                    output = selectedToken.ToObject<bool>();
                }
                if (selectedToken.Type == JTokenType.Integer)
                {
                    output = selectedToken.ToObject<Int64>();
                }
                if (selectedToken.Type == JTokenType.Float)
                {
                    output = selectedToken.ToObject<float>();
                }

            }
            return output;
        }

        #region grouparrayby
        public static object grouparrayby(string jsonPath, string groupingElement, string groupedElement, JUSTContext context)
        {
            JToken inObj = context.Input;
            JArray arr = (JArray)inObj.SelectToken(jsonPath);
            if (!groupingElement.Contains(":"))
            {
                JArray result = Utilities.GroupArray(arr, groupingElement, groupedElement);
                return JsonConvert.SerializeObject(result);
            }
            else
            {
                string[] groupingElements = groupingElement.Split(':');
                JArray result = Utilities.GroupArrayMultipleProperties(arr, groupingElements, groupedElement);
                return JsonConvert.SerializeObject(result);
            }
        }

        #endregion

        #region operators
        public static bool stringequals(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                if (list[0].ToString().Equals(list[1].ToString()))
                    result = true;
            }

            return result;
        }

        public static bool stringcontains(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                if (list[0].ToString().Contains(list[1].ToString()))
                    result = true;
            }

            return result;
        }

        public static bool mathequals(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[0], EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[1], EvaluationMode.Strict);

                result = lshDecimal == rhsDecimal;
            }

            return result;
        }

        public static bool mathgreaterthan(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[0], EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[1], EvaluationMode.Strict);

                result = lshDecimal > rhsDecimal;
            }

            return result;
        }

        public static bool mathlessthan(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[0], EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[1], EvaluationMode.Strict);

                result = lshDecimal < rhsDecimal;
            }

            return result;
        }

        public static bool mathgreaterthanorequalto(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[0], EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[1], EvaluationMode.Strict);

                result = lshDecimal >= rhsDecimal;
            }

            return result;
        }

        public static bool mathlessthanorequalto(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[0], EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[1], EvaluationMode.Strict);

                result = lshDecimal <= rhsDecimal;
            }

            return result;
        }
        #endregion

        public static object tointeger(object val, JUSTContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(int), val, context.EvaluationMode);
        }

        public static object tostring(object val, JUSTContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(string), val, context.EvaluationMode);
        }

        public static object toboolean(object val, JUSTContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(bool), val, context.EvaluationMode);
        }

        public static decimal todecimal(object val, JUSTContext context)
        {
            return decimal.Round((decimal)ReflectionHelper.GetTypedValue(typeof(decimal), val, context.EvaluationMode), context.DefaultDecimalPlaces);
        }

        public static decimal round(decimal val, int decimalPlaces, JUSTContext context)
        {
            return decimal.Round(val, decimalPlaces);
        }
    }
}
