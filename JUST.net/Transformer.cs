using JUST.net.Selectables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;

namespace JUST
{
    public abstract class Transformer
    {
        protected int _loopCounter = 0;

        protected readonly JUSTContext Context;

        public Transformer(JUSTContext context)
        {
            Context = context ?? new JUSTContext();
        }

        protected static object TypedNumber(decimal number)
        {
            return number * 10 % 10 == 0 ? (number <= int.MaxValue ? (object)Convert.ToInt32(number) : number) : number;
        }

        
        internal static JToken GetToken<T>(JToken input, string path, IContext context) where T : ISelectableToken
        {
            T selector = context.Resolve<T>(input);
            return selector.Select(path);
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

    public abstract class Transformer<T> : Transformer where T : ISelectableToken
    {
        public Transformer(JUSTContext context) : base(context)
        {
        }

        public static object valueof(string path, JToken input, IContext context)
        {
            JToken selectedToken = GetToken<T>(input, path, context);
            return GetValue(selectedToken);
        }

        public static bool exists(string path, JToken input, IContext context)
        {
            JToken selectedToken = GetToken<T>(input, path, context);
            return selectedToken != null;
        }

        public static bool existsandnotempty(string path, JToken input, IContext context)
        {
            JToken selectedToken = GetToken<T>(input, path, context);
            return selectedToken != null && (
                (selectedToken.Type == JTokenType.String && selectedToken.ToString().Trim() != string.Empty) ||
                (selectedToken.Type == JTokenType.Array && selectedToken.Children().Count() > 0)
            );
        }

        public static object ifcondition(object condition, object value, object trueResult, object falseResult, JToken input, IContext context)
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

        public static object concat(object obj1, object obj2, JToken input, IContext context)
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

        public static string substring(string stringRef, int startIndex, int length, JToken input, IContext context)
        {
            try
            {
                return stringRef.Substring(startIndex, length);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleException(ex, context.IsStrictMode());
            }
            return null;
        }

        public static int firstindexof(string stringRef, string searchString, JToken input, IContext context)
        {
            return stringRef.IndexOf(searchString, 0);
        }

        public static int lastindexof(string stringRef, string searchString, JToken input, IContext context)
        {
            return stringRef.LastIndexOf(searchString);
        }

        public static string concatall(object obj, JToken input, IContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Concatall(JToken.FromObject(valueof(path, input, context)), context);
            }
            else
            {
                return Concatall(token, context);
            }
        }

        private static string Concatall(JToken parsedArray, IContext context)
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

        public static string concatallatpath(JArray parsedArray, string path, JToken input, IContext context)
        {
            string result = null;

            if (parsedArray != null)
            {
                result = string.Empty;
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = GetToken<T>(token, path, context);
                    if (context.IsStrictMode() && selectedToken.Type != JTokenType.String)
                    {
                        throw new Exception($"Invalid value in array to concatenate: {selectedToken.ToString()}");
                    }
                    result += selectedToken?.ToString();
                }
            }

            return result;
        }

        #endregion

        #region math functions


        public static object add(decimal num1, decimal num2, JToken input, IContext context)
        {
            return TypedNumber(num1 + num2);
        }

        public static object subtract(decimal num1, decimal num2, JToken input, IContext context)
        {
            return TypedNumber(num1 - num2);
        }
        public static object multiply(decimal num1, decimal num2, JToken input, IContext context)
        {
            return TypedNumber(num1 * num2);
        }
        public static object divide(decimal num1, decimal num2, JToken input, IContext context)
        {
            return TypedNumber(num1 / num2);
        }
        #endregion

        #region aggregate functions
        public static object sum(object obj, JToken input, IContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Sum(JToken.FromObject(valueof(path, input, context)));
            }
            else
            {
                return Sum(token);
            }
        }

        private static object Sum(JToken parsedArray)
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

        public static object sumatpath(JArray parsedArray, string path, JToken input, IContext context)
        {
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = GetToken<T>(token, path, context);
                    result += Convert.ToDecimal(selectedToken.ToString());
                    // var selector = context.Resolve<T>(token);
                    // if (selector.Select(path) is JToken selectedToken)
                    // {
                    //     result += Convert.ToDecimal(selectedToken.ToString());
                    // }
                }
            }

            return TypedNumber(result);
        }

        public static object average(object obj, JToken input, IContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Average(JToken.FromObject(valueof(path, input, context)));
            }
            else
            {
                return Average(token);
            }
        }

        private static object Average(JToken token)
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

        public static object averageatpath(JArray parsedArray, string path, JToken input, IContext context)
        {
            decimal result = 0;

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = GetToken<T>(token, path, context);
                    result += Convert.ToDecimal(selectedToken.ToString());
                    // var selector = context.Resolve<T>(token);
                    // if (selector.Select(path) is JToken selectedToken)
                    // {
                    //     result += Convert.ToDecimal(selectedToken.ToString());
                    // }
                }
            }

            return TypedNumber(result / parsedArray.Count);
        }

        public static object max(object obj, JToken input, IContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Max(JToken.FromObject(valueof(path, input, context)));
            }
            else
            {
                return Max(token);
            }
        }

        private static object Max(JToken token)
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
            decimal thisValue = Convert.ToDecimal(token?.ToString());
            return Math.Max(d1, thisValue);
        }

        public static object maxatpath(JArray parsedArray, string path, JToken input, IContext context)
        {
            decimal result = 0;
            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = GetToken<T>(token, path, context);
                    result = Max(result, selectedToken);
                }
            }

            return TypedNumber(result);
        }

        public static object min(object obj, JToken input, IContext context)
        {
            JToken token = JToken.FromObject(obj);
            if (obj is string path && path.StartsWith(context.Resolve<T>(token).RootReference))
            {
                return Min(JToken.FromObject(valueof(path, input, context)));
            }
            else
            {
                return Min(token);
            }
        }

        private static object Min(JToken token)
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

        public static object minatpath(JArray parsedArray, string path, JToken input, IContext context)
        {
            decimal? result = null;

            if (parsedArray != null)
            {
                foreach (JToken token in parsedArray.Children())
                {
                    JToken selectedToken = GetToken<T>(token, path, context);
                    decimal thisValue = Convert.ToDecimal(selectedToken.ToString());
                    result = Math.Min(result ?? decimal.MaxValue, thisValue);
                }
            }

            return TypedNumber(result ?? decimal.MaxValue);
        }

        public static int arraylength(string array, JToken input, IContext context)
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

        public static object currentvalueatpath(JArray array, JToken currentElement, string path, IContext context)
        {
            JToken selectedToken = GetToken<T>(currentElement, path, context);
            return GetValue(selectedToken);
        }

        public static object currentproperty(JArray array, JToken currentElement, IContext context)
        {
            var prop = (currentElement.First as JProperty);
            if (prop == null && context.IsStrictMode())
            {
                throw new InvalidOperationException("Element is not a property: " + prop.ToString());
            }
            return prop.Name;
        }

        public static object lastvalueatpath(JArray array, JToken currentElement, string path, IContext context)
        {
            JToken selectedToken = GetToken<T>(array.Last(), path, context);
            return GetValue(selectedToken);
        }
        #endregion

        #region Constants

        public static string constant_comma(JToken input, IContext context)
        {
            return ",";
        }

        public static string constant_hash(JToken input, IContext context)
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
                    result = concat(result, list[i], null, null);
                }
            }

            return result;
        }

        public static object xadd(object[] list)
        {
            IContext context = list[list.Length - 1] as IContext;
            decimal add = 0;
            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i] != null)
                    add += (decimal)ReflectionHelper.GetTypedValue(typeof(decimal), list[i], context.IsStrictMode());
            }

            return TypedNumber(add);
        }
        #endregion

        #region grouparrayby
        public static JArray grouparrayby(string path, string groupingElement, string groupedElement, JToken input, IContext context)
        {
            JArray result;
            JArray arr = (JArray)GetToken<T>(input, path, context);
            if (!groupingElement.Contains(context.SplitGroupChar))
            {
                result = Utilities.GroupArray<T>(arr, groupingElement, groupedElement, context);
            }
            else
            {
                string[] groupingElements = groupingElement.Split(context.SplitGroupChar);
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
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[1],
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);

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
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[1],
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);

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
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[1],
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);

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
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[1],
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);

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
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);
                decimal rhsDecimal = (decimal)ReflectionHelper.GetTypedValue(typeof(decimal),
                    list[1],
                    list.Length >= 3 ? ((IContext)list[2]).IsStrictMode() : true);

                result = lshDecimal <= rhsDecimal;
            }

            return result;
        }
        #endregion

        public static object tointeger(object val, JToken input, IContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(int), val, context.IsStrictMode());
        }

        public static object tostring(object val, JToken input, IContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(string), val, context.IsStrictMode());
        }

        public static object toboolean(object val, JToken input, IContext context)
        {
            return ReflectionHelper.GetTypedValue(typeof(bool), val, context.IsStrictMode());
        }

        public static decimal todecimal(object val, JToken input, IContext context)
        {
            return decimal.Round((decimal)ReflectionHelper.GetTypedValue(typeof(decimal), val, context.IsStrictMode()), context.DefaultDecimalPlaces);
        }

        public static decimal round(decimal val, int decimalPlaces, JToken input, IContext context)
        {
            return decimal.Round(val, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        public static int length(object val, JToken input, IContext context)
        {
            int result = 0;
            if (val is string path && path.StartsWith(context.Resolve<T>(null).RootReference))
            {
                result = length(valueof(path, input, context), input, context);
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
        public static bool isnumber(object val, JToken input, IContext context)
        {
            try
            {
                object r = ReflectionHelper.GetTypedValue(typeof(decimal), val, context.IsStrictMode());
                return r is decimal;
            }
            catch
            {
                return false;
            }
        }

        public static bool isboolean(object val, JToken input, IContext context)
        {
            return val != null ? val.GetType() == typeof(bool) : false;
        }

        public static bool isstring(object val, JToken input, IContext context)
        {
            return val != null ? val.GetType() == typeof(string) : false;
        }

        public static bool isarray(object val, JToken input, IContext context)
        {
            return val != null ? val.GetType().IsArray : false;
        }

        public static object ifgroup(bool isToInclude, object val)
        {
            return isToInclude ? val : null;
        }

        public static string stringempty()
        {
            return string.Empty;
        }

        public static object arrayempty(JToken input, IContext context)
        {
            return Array.Empty<object>();
        }
    }
}
