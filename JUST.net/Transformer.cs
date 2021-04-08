using JUST.net.Selectables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;

namespace JUST
{
    internal class Transformer
    {
        protected static object TypedNumber(decimal number)
        {
            return number * 10 % 10 == 0 ? (number <= int.MaxValue ? (object)Convert.ToInt32(number) : number) : number;
        }

        internal static object GetValue(JToken selectedToken)
        {
            object output = null;
            if (selectedToken != null)
            {
                switch (selectedToken.Type)
                {
                    case JTokenType.Object:
                        output = selectedToken;
                        break;
                    case JTokenType.Array:
                        output = selectedToken.Values<object>().ToArray();
                        break;
                    case JTokenType.Integer:
                        output = selectedToken.ToObject<long>();
                        break;
                    case JTokenType.Float:
                        output = selectedToken.ToObject<double>();
                        break;
                    case JTokenType.String:
                        output = selectedToken.ToString();
                        break;
                    case JTokenType.Boolean:
                        output = selectedToken.ToObject<bool>();
                        break;
                    case JTokenType.Date:
                        DateTime value = selectedToken.Value<DateTime>();
                        output = value.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
                        break;
                    case JTokenType.Raw:
                        break;
                    case JTokenType.Bytes:
                        output = selectedToken.Value<byte[]>();
                        break;
                    case JTokenType.Guid:
                        output = selectedToken.Value<Guid>();
                        break;
                    case JTokenType.TimeSpan:
                        output = selectedToken.Value<TimeSpan>();
                        break;
                    case JTokenType.Uri:
                    case JTokenType.Undefined:
                    case JTokenType.Constructor:
                    case JTokenType.Property:
                    case JTokenType.Comment:
                    case JTokenType.Null:
                    case JTokenType.None:
                        break;
                    default:
                        break;
                }
            }
            return output;
        }
    }

    internal class Transformer<T> : Transformer where T : ISelectableToken
    {
        public static object valueof(string path, JUSTContext context)
        {
            var selector = context.Resolve<T>(context.Input);
            JToken selectedToken = selector.Select(path);
            return GetValue(selectedToken);
        }

        public static bool exists(string path, JUSTContext context)
        {
            var selector = context.Resolve<T>(context.Input);
            JToken selectedToken = selector.Select(path);
            return selectedToken != null;
        }

        public static bool existsandnotempty(string path, JUSTContext context)
        {
            var selector = context.Resolve<T>(context.Input);
            JToken selectedToken = selector.Select(path);
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
        private static object ConcatArray(object obj1, object obj2)
        {
            JArray item = new JArray();
            JToken item1 = null, item2 = null;
            for (int i = 0; i < ((object[])obj1).Length; i++)
            {
                if (((object[])obj1)[i] is JValue)
                {
                    item1 = (JValue)((object[])obj1)[i];
                    item.Add(item1);
                }
                else
                {
                    item1 = (JObject)((object[])obj1)[i];
                    item.Add(item1);
                }
            }
            for (int j = 0; obj2 != null && j < ((object[])obj2).Length; j++)
            {
                if (((object[])obj2)[j] is JValue)
                {
                    item2 = (JValue)((object[])obj2)[j];
                    item1.AddAfterSelf(item2);
                }
                else
                {
                    item2 = (JObject)((object[])obj2)[j];
                    item.Add(item2);
                }
            }
            return item.ToObject<object[]>();
        }

        public static object concat(object obj1, object obj2, JUSTContext context)
        {
            if (obj1 != null)
            {
                if (obj1 is string str1)
                {
                    return str1.Length > 0 ? str1 + obj2?.ToString() : string.Empty + obj2.ToString();
                }
                return ConcatArray(obj1, obj2);
            }
            else if (obj2 != null)
            {
                if (obj2 is string str2)
                {
                    return str2.Length > 0 ? obj1?.ToString() + str2 : obj1.ToString() + string.Empty;
                }
                return ConcatArray(obj2, obj1);
            }

            return null;
        }

        public static string substring(string stringRef, int startIndex, int length, JUSTContext context)
        {
            try
            {
                return stringRef.Substring(startIndex, length);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleException(ex, context.EvaluationMode);
            }
            return null;
        }

        public static int firstindexof(string stringRef, string searchString, JUSTContext context)
        {
            return stringRef.IndexOf(searchString, 0);
        }

        public static int lastindexof(string stringRef, string searchString, JUSTContext context)
        {
            return stringRef.LastIndexOf(searchString);
        }

        public static string concatall(object obj, JUSTContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Concatall(JToken.FromObject(valueof(path, context)), context);
            }
            else
            {
                return Concatall(token, context);
            }
        }

        private static string Concatall(JToken parsedArray, JUSTContext context)
        {
            string result = null;

            if (parsedArray != null)
            {
                if (result == null)
                {
                    result = string.Empty;
                }
                foreach (JToken token in parsedArray.Children())
                {
                    if (context.IsStrictMode() && token.Type != JTokenType.String)
                    {
                        throw new Exception($"Invalid value in array to concatenate: {token.ToString()}");
                    }
                    result += token.ToString();
                }
            }

            return result;
        }

        public static string concatallatpath(JArray parsedArray, string path, JUSTContext context)
        {
            string result = null;

            if (parsedArray != null)
            {
                result = string.Empty;
                foreach (JToken token in parsedArray.Children())
                {
                    var selector = context.Resolve<T>(token);
                    JToken selectedToken = selector.Select(path);
                    if (context.IsStrictMode() && selectedToken.Type != JTokenType.String)
                    {
                        throw new Exception($"Invalid value in array to concatenate: {selectedToken.ToString()}");
                    }
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
        #endregion

        #region aggregate functions
        public static object sum(object obj, JUSTContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Sum(JToken.FromObject(valueof(path, context)), context);
            }
            else
            {
                return Sum(token, context);
            }
        }

        private static object Sum(JToken parsedArray, JUSTContext context)
        {
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

        public static object sumatpath(JArray parsedArray, string path, JUSTContext context)
        {
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    var selector = context.Resolve<T>(token);
                    JToken selectedToken = selector.Select(path);
                    result += Convert.ToDecimal(selectedToken.ToString());
                }
            }

            return TypedNumber(result);
        }

        public static object average(object obj, JUSTContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Average(JToken.FromObject(valueof(path, context)), context);
            }
            else
            {
                return Average(token, context);
            }
        }

        private static object Average(JToken token, JUSTContext context)
        {
            decimal result = 0;
            JArray parsedArray = token as JArray;
            if (token != null)
            {
                foreach (JToken child in token.Children())
                {
                    result += Convert.ToDecimal(child.ToString());
                }
            }

            return TypedNumber(result / parsedArray.Count);
        }

        public static object averageatpath(JArray parsedArray, string path, JUSTContext context)
        {
            decimal result = 0;

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    var selector = context.Resolve<T>(token);
                    JToken selectedToken = selector.Select(path);
                    result += Convert.ToDecimal(selectedToken.ToString());
                }
            }

            return TypedNumber(result / parsedArray.Count);
        }

        public static object max(object obj, JUSTContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Max(JToken.FromObject(valueof(path, context)), context);
            }
            else
            {
                return Max(token, context);
            }
        }

        private static object Max(JToken token, JUSTContext context)
        {
            decimal result = 0;
            if (token != null)
            {
                foreach (JToken child in token.Children())
                {
                    result = Max(result, child);
                }
            }

            return TypedNumber(result);
        }

        private static decimal Max(decimal d1, JToken token)
        {
            decimal thisValue = Convert.ToDecimal(token.ToString());
            return Math.Max(d1, thisValue);
        }

        public static object maxatpath(JArray parsedArray, string path, JUSTContext context)
        {
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    var selector = context.Resolve<T>(token);
                    JToken selectedToken = selector.Select(path);
                    result = Max(result, selectedToken);
                }
            }

            return TypedNumber(result);
        }

        public static object min(object obj, JUSTContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Min(JToken.FromObject(valueof(path, context)), context);
            }
            else
            {
                return Min(token, context);
            }
        }

        private static object Min(JToken token, JUSTContext context)
        {
            decimal result = decimal.MaxValue;
            if (token != null)
            {
                foreach (JToken child in token.Children())
                {
                    decimal thisValue = Convert.ToDecimal(child.ToString());
                    result = Math.Min(result, thisValue);
                }
            }

            return TypedNumber(result);
        }

        public static object minatpath(JArray parsedArray, string path, JUSTContext context)
        {
            decimal result = decimal.MaxValue;

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    var selector = context.Resolve<T>(token);
                    JToken selectedToken = selector.Select(path);
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

        public static object currentvalueatpath(JArray array, JToken currentElement, string path, JUSTContext context)
        {
            var selector = context.Resolve<T>(currentElement);
            JToken selectedToken = selector.Select(path);
            return GetValue(selectedToken);
        }

        public static object currentproperty(JArray array, JToken currentElement, JUSTContext context)
        {
            var prop = (currentElement.First as JProperty);
            if (prop == null && context.IsStrictMode())
            {
                throw new InvalidOperationException("Element is not a property: " + prop.ToString());
            }
            return prop.Name;
        }

        public static object lastvalueatpath(JArray array, JToken currentElement, string path, JUSTContext context)
        {
            var selector = context.Resolve<T>(array.Last);
            JToken selectedToken = selector.Select(path);
            return GetValue(selectedToken);
        }
        #endregion

        #region Constants

        public static string constant_comma(JUSTContext context)
        {
            return ",";
        }

        public static string constant_hash(JUSTContext context)
        {
            return "#";
        }

        #endregion

        #region Variable parameter functions
        public static object xconcat(object[] list)
        {
            object result = null;

            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i] != null)
                {
                    result = concat(result, list[i], null);
                }
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
        
        #region grouparrayby
        public static JArray grouparrayby(string path, string groupingElement, string groupedElement, JUSTContext context)
        {
            JArray result;
            var selector = context.Resolve<T>(context.Input);
            JArray arr = (JArray)selector.Select(path);
            if (!groupingElement.Contains(":"))
            {
                result = Utilities.GroupArray<T>(arr, groupingElement, groupedElement, context);
            }
            else
            {
                string[] groupingElements = groupingElement.Split(':');
                result = Utilities.GroupArrayMultipleProperties<T>(arr, groupingElements, groupedElement, context);
            }
            return result;
        }

        #endregion

        #region operators
        public static bool stringequals(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                var context = (list.Length > 2)
                    ? (JUSTContext)list[2]
                    : new JUSTContext();

                if (ComparisonHelper.Equals(list[0], list[1], context.EvaluationMode))
                    result = true;
            }

            return result;
        }

        public static bool stringcontains(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                var context = (list.Length > 2)
                    ? (JUSTContext)list[2]
                    : new JUSTContext();

                result = ComparisonHelper.Contains(list[0], list[1], context.EvaluationMode);
            }

            return result;
        }

        public static bool mathequals(object[] list)
        {
            bool result = false;

            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[0], 
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[1],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);

                result = lshDecimal == rhsDecimal;
            }

            return result;
        }

        public static bool mathgreaterthan(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[0],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[1],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);

                result = lshDecimal > rhsDecimal;
            }

            return result;
        }

        public static bool mathlessthan(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[0],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[1],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);

                result = lshDecimal < rhsDecimal;
            }

            return result;
        }

        public static bool mathgreaterthanorequalto(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[0],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[1],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);

                result = lshDecimal >= rhsDecimal;
            }

            return result;
        }

        public static bool mathlessthanorequalto(object[] list)
        {
            bool result = false;
            if (list.Length >= 2)
            {
                decimal lshDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[0],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), 
                    list[1],
                    list.Length >= 3 ? ((JUSTContext)list[2]).EvaluationMode : EvaluationMode.Strict);

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
            return decimal.Round(val, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        public static int length(object val, JUSTContext context)
        {
            int result = 0;
            if (val is string path && path.StartsWith(context.Resolve<T>(null).RootReference))
            {
                result = length(valueof(path, context), context);
            }
            else if (val is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    result++;
                }
            }
            else
            {
                if (context.IsStrictMode())
                {
                    throw new ArgumentException($"Argument not elegible for #length: {val}");
                }
            }
            return result;
        }
    }
}
