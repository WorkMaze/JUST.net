using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;

namespace JUST
{
    internal class Transformer
    {
        public static object valueof(string jsonPath, string inputJson)
        {
            JToken token = JsonConvert.DeserializeObject<JObject>(inputJson);
            JToken selectedToken = token.SelectToken(jsonPath);
            return GetValue(selectedToken);
        }

        public static string exists(string jsonPath, string inputJson)
        {
            JToken token = JsonConvert.DeserializeObject<JObject>(inputJson);
            JToken selectedToken = token.SelectToken(jsonPath);

            if (selectedToken != null)
                return "true";
            else
                return "false";
        }

        public static string existsandnotempty(string jsonPath, string inputJson)
        {
            JToken token = JsonConvert.DeserializeObject<JObject>(inputJson);
            JToken selectedToken = token.SelectToken(jsonPath);

            if (selectedToken != null)
            {
                if (selectedToken.ToString().Trim() != string.Empty)
                    return "true";
                else
                    return "false";
            }
            else
                return "false";
        }

        public static object ifcondition(object condition, object value, object trueResult, object falseResult, string inputJson)
        {
            object output = falseResult;

            if (condition.ToString().ToLower() == value.ToString().ToLower())
                output = trueResult;

            return output;
        }

        #region string functions

        public static string concat(string string1, string string2, string inputJson)
        {
            string string2Result = (string2 != null) ? string2 : string.Empty;
            return string1 != null ? string1 + string2Result : string.Empty + string2Result;
        }

        public static string substring(string stringRef, int startIndex, int length, string inputJson)
        {
            try
            {
                return stringRef.Substring(Convert.ToInt32(startIndex), Convert.ToInt32(length));
            }
            catch
            {
                return null;
            }
        }

        public static int firstindexof(string stringRef, string searchString, string inputJson)
        {
            return stringRef.IndexOf(searchString, 0);
        }

        public static int lastindexof(string stringRef, string searchString, string inputJson)
        {
            return stringRef.LastIndexOf(searchString);
        }

        public static string concatall(string array, string inputJson)
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

        public static string concatallatpath(string array, string jsonPath, string inputJson)
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


        public static double add(double num1, double num2, string inputJson)
        {
            return num1 + num2;
        }
        public static double subtract(double num1, double num2, string inputJson)
        {
            return num1 - num2;
        }
        public static double multiply(double num1, double num2, string inputJson)
        {
            return num1 * num2;
        }
        public static double divide(double num1, double num2, string inputJson)
        {
            return num1 / num2;
        }
        #endregion

        #region aggregate functions
        public static double sum(string array, string inputJson)
        {
            JArray parsedArray = JArray.Parse(array);

            double integerresult = 0;

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {

                    integerresult += Convert.ToDouble(token.ToString());
                }
            }

            return integerresult;
        }

        public static double sumatpath(string array, string jsonPath, string inputJson)
        {
            double integerresult = 0;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {

                foreach (JToken token in parsedArray.Children())
                {

                    JToken selectedToken = token.SelectToken(jsonPath);


                    integerresult += Convert.ToDouble(selectedToken.ToString());
                }
            }

            return integerresult;
        }

        public static double average(string array, string inputJson)
        {
                JArray parsedArray = JArray.Parse(array);

                double integerresult = 0;

                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        integerresult += Convert.ToDouble(token.ToString());
                    }
                }

                return (double)integerresult / (double)parsedArray.Count;
        }

        public static double averageatpath(string array, string jsonPath, string inputJson)
        {
            double integerresult = 0;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {

                foreach (JToken token in parsedArray.Children())
                {

                    JToken selectedToken = token.SelectToken(jsonPath);


                    integerresult += Convert.ToDouble(selectedToken.ToString());
                }
            }

            return (double)integerresult / (double)parsedArray.Count;
        }

        public static double max(string array, string inputJson)
        {
            JArray parsedArray = JArray.Parse(array);

            double integerresult = 0;
            int i = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {

                    double thisValue = Convert.ToDouble(token.ToString());

                    if (i == 0)
                        integerresult = thisValue;
                    else
                    {
                        if (integerresult < thisValue)
                            integerresult = thisValue;
                    }

                    i++;
                }
            }

            return integerresult;
        }

        public static double maxatpath(string array, string jsonPath, string inputJson)
        {
            double integerresult = 0;
            int i = 0;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {

                foreach (JToken token in parsedArray.Children())
                {

                    JToken selectedToken = token.SelectToken(jsonPath);


                    double thisValue = Convert.ToDouble(selectedToken.ToString());

                    if (i == 0)
                        integerresult = thisValue;
                    else
                    {
                        if (integerresult < thisValue)
                            integerresult = thisValue;
                    }

                    i++;
                }
            }

            return integerresult;
        }


        public static double min(string array, string inputJson)
        {
                JArray parsedArray = JArray.Parse(array);

                double integerresult = 0;
                int i = 0;
                if (parsedArray != null)
                {
                    foreach (JToken token in parsedArray.Children())
                    {

                        double thisValue = Convert.ToDouble(token.ToString());

                        if (i == 0)
                            integerresult = thisValue;
                        else
                        {
                            if (integerresult > thisValue)
                                integerresult = thisValue;
                        }

                        i++;
                    }
                }

                return integerresult;
        }

        public static double minatpath(string array, string jsonPath, string inputJson)
        {
            double integerresult = 0;
            int i = 0;

            JArray parsedArray = JArray.Parse(array);

            if (parsedArray != null)
            {

                foreach (JToken token in parsedArray.Children())
                {

                    JToken selectedToken = token.SelectToken(jsonPath);


                    double thisValue = Convert.ToDouble(selectedToken.ToString());

                    if (i == 0)
                        integerresult = thisValue;
                    else
                    {
                        if (integerresult > thisValue)
                            integerresult = thisValue;
                    }

                    i++;
                }
            }

            return integerresult;
        }

        public static int arraylength(string array, string inputJson)
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

        public static string constant_comma(string none, string inputJson)
        {
            return ",";
        }

        public static string constant_hash(string none, string inputJson)
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

        public static double xadd(object[] list)
        {
            double add = 0;

            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i] != null)
                    add += (double)ReflectionHelper.GetTypedValue(typeof(double), list[i], EvaluationMode.Loose);
            }

            return add;
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
                    output = value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
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
        public static object grouparrayby(string jsonPath, string groupingElement, string groupedElement, string inputJson)
        {
            if (!groupingElement.Contains(":"))
            {

                JObject inObj = JsonConvert.DeserializeObject<JObject>(inputJson);

                JArray arr = (JArray)inObj.SelectToken(jsonPath);

                JArray result = Utilities.GroupArray(arr, groupingElement, groupedElement);

                return JsonConvert.SerializeObject(result);
            }
            else
            {
                string[] groupingElements = groupingElement.Split(':');

                JObject inObj = JsonConvert.DeserializeObject<JObject>(inputJson);

                JArray arr = (JArray)inObj.SelectToken(jsonPath);

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

    }
}
