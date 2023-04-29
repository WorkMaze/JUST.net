using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JUST.net.Selectables;

namespace JUST
{
    public class JsonTransformer : JsonTransformer<JsonPathSelectable>
    {
        public JsonTransformer(JUSTContext context = null) : base(context)
        {
        }
    }

    public class JsonTransformer<T> : Transformer<T> where T : ISelectableToken
    {
        public JsonTransformer(JUSTContext context = null) : base(context)
        {
        }
        public string Transform(string transformerJson, string inputJson)
        {
            return Transform(transformerJson, DeserializeWithoutDateParse<JToken>(inputJson));
        }

        private static string SerializeWithoutDateParse<T>(T obj)
        {
            var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
            return JsonConvert.SerializeObject(obj, settings);
        }

        private static T DeserializeWithoutDateParse<T>(string inputJson)
        {
            var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
            return JsonConvert.DeserializeObject<T>(inputJson, settings);
        }

        public string Transform(string transformerJson, JToken input)
        {
            JToken result;
            JToken transformerToken = DeserializeWithoutDateParse<JToken>(transformerJson);
            switch (transformerToken.Type)
            {
                case JTokenType.Object:
                    result = Transform(transformerToken as JObject, input);
                    break;
                case JTokenType.Array:
                    result = Transform(transformerToken as JArray, input);
                    break;
                default:
                    result = TransformValue(transformerToken, input);
                    break;
            }

            string output = SerializeWithoutDateParse(result);

            return output;
        }

        public JArray Transform(JArray transformerArray, string input)
        {
            return Transform(transformerArray, DeserializeWithoutDateParse<JToken>(input));
        }

        public JArray Transform(JArray transformerArray, JToken input)
        {
            var result = new JArray();
            var nr = transformerArray.Count;
            for (int i = 0; i < nr; i++)
            {
                var transformer = transformerArray[i];
                if (transformer.Type == JTokenType.Object)
                {
                    var t = Transform(transformer as JObject, input);
                    result.Add(t);
                }
                else
                {
                    var token = TransformValue(transformer, input);
                    foreach (var item in token)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        private JToken TransformValue(JToken transformer, JToken input)
        {
            var tmp = new JObject
            {
                { "root", transformer }
            };
            Transform(tmp, input);
            return tmp["root"];
        }

        public JToken Transform(JObject transformer, string input)
        {
            return Transform(transformer, DeserializeWithoutDateParse<JToken>(input));
        }

        public JToken Transform(JObject transformer, JToken input)
        {
            Context.Input = input;
            var parentToken = (JToken)transformer;
            RecursiveEvaluate(ref parentToken, null, null);
            return parentToken;
        }

        #region RecursiveEvaluate


        private void RecursiveEvaluate(ref JToken parentToken, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken)
        {
            if (parentToken == null)
            {
                return;
            }

            JEnumerable<JToken> tokens = parentToken.Children();

            List<JToken> selectedTokens = null;
            Dictionary<string, JToken> tokensToReplace = null;
            List<JToken> tokensToDelete = null;

            List<string> loopProperties = null;
            List<string> condProps = null;
            JArray arrayToForm = null;
            JObject dictToForm = null;
            List<JToken> tokenToForm = null;
            List<JToken> tokensToAdd = null;

            bool isLoop = false;
            bool isBulk = false;

            foreach (JToken childToken in tokens)
            {
                ParseToken(parentToken, parentArray, currentArrayToken, ref selectedTokens, ref tokensToReplace, ref tokensToDelete, ref loopProperties, ref condProps, ref arrayToForm, ref dictToForm, ref tokenToForm, ref tokensToAdd, ref isLoop, ref isBulk, childToken);
            }

            if (selectedTokens != null)
            {
                CopyPostOperationBuildUp(parentToken, selectedTokens);
            }
            if (tokensToReplace != null)
            {
                ReplacePostOperationBuildUp(parentToken, tokensToReplace);
            }
            if (tokensToDelete != null)
            {
                DeletePostOperationBuildUp(parentToken, tokensToDelete);
            }
            if (tokensToAdd != null)
            {
                AddPostOperationBuildUp(parentToken, tokensToAdd);
            }
            PostOperationsBuildUp(ref parentToken, tokenToForm);
            if (loopProperties != null || condProps != null)
            {
                LoopPostOperationBuildUp(ref parentToken, condProps, loopProperties, arrayToForm, dictToForm);
            }
        }

        private void ParseToken(JToken parentToken, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<JToken> selectedTokens, ref Dictionary<string, JToken> tokensToReplace, ref List<JToken> tokensToDelete, ref List<string> loopProperties, ref List<string> condProps, ref JArray arrayToForm, ref JObject dictToForm, ref List<JToken> tokenToForm, ref List<JToken> tokensToAdd, ref bool isLoop, ref bool isBulk, JToken childToken)
        {
            if (childToken.Type == JTokenType.Array && (parentToken as JProperty)?.Name.Trim() != "#")
            {
                IEnumerable<object> itemsToAdd = TransformArray(childToken.Children(), parentArray, currentArrayToken);
                BuildArrayToken(childToken as JArray, itemsToAdd);
            }
            else if (childToken.Type == JTokenType.Property && childToken is JProperty property && property.Name != null)
            {
                /* For looping*/
                isLoop = false;

                if (property.Name == "#" && property.Value.Type == JTokenType.Array && property.Value is JArray values)
                {
                    BulkOperations(values.Children(), parentArray, currentArrayToken, ref selectedTokens, ref tokensToReplace, ref tokensToDelete);
                    isBulk = true;
                }
                else
                {
                    isBulk = false;
                    if (ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments))
                    {
                        ParsePropertyFunction(parentArray, currentArrayToken, ref loopProperties, ref condProps, ref arrayToForm, ref dictToForm, ref tokenToForm, ref tokensToAdd, ref isLoop, childToken, property, functionName, arguments);
                    }
                    else if (property.Value.ToString().Trim().StartsWith("#"))
                    {
                        property.Value = GetToken(ParseFunction(property.Value.ToString().Trim(), parentArray, currentArrayToken));
                    }
                }

                if (property.Name != null && property.Value.ToString().StartsWith($"{Context.EscapeChar}#"))
                {
                    var clone = property.Value as JValue;
                    clone.Value = clone.Value.ToString().Substring(1);
                    property.Value.Replace(clone);
                }
                /*End looping */
            }
            else if (childToken.Type == JTokenType.String && childToken.Value<string>().Trim().StartsWith("#")
                && parentArray != null && currentArrayToken != null)
            {
                object newValue = ParseFunction(childToken.Value<string>(), parentArray, currentArrayToken);
                childToken.Replace(GetToken(newValue));
            }

            if (!isLoop && !isBulk)
            {
                RecursiveEvaluate(ref childToken, parentArray, currentArrayToken);
            }
        }

        private void ParsePropertyFunction(IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<string> loopProperties, ref List<string> condProps, ref JArray arrayToForm, ref JObject dictToForm, ref List<JToken> tokenToForm, ref List<JToken> tokensToAdd, ref bool isLoop, JToken childToken, JProperty property, string functionName, string arguments)
        {
            switch (functionName)
            {
                case "ifgroup":
                    ConditionalGroupOperation(property.Name, arguments, parentArray, currentArrayToken, ref condProps, ref tokenToForm, childToken);
                    break;
                case "loop":
                    LoopOperation(property.Name, arguments, parentArray, currentArrayToken, ref loopProperties, ref arrayToForm, ref dictToForm, childToken);
                    isLoop = true;
                    break;
                case "eval":
                    EvalOperation(property, arguments, parentArray, currentArrayToken, ref loopProperties, ref tokensToAdd);
                    break;
            }
        }

        private void PostOperationsBuildUp(ref JToken parentToken, List<JToken> tokenToForm)
        {
            if (tokenToForm != null)
            {
                foreach (JToken token in tokenToForm)
                {
                    foreach (JToken childToken in token.Children())
                    {
                        if (childToken is JProperty child)
                        {
                            (parentToken as JObject).Add(child.Name, child.Value);
                        }
                        else if (token is JArray arr && parentToken.Parent != null)
                        {
                            (parentToken.Parent as JProperty).Value = arr;
                        }
                        else
                        {
                            if (Context.IsStrictMode())
                            {
                                throw new Exception($"found {parentToken.Type} without parent!");
                            }
                        }
                    }
                }
            }
            if (parentToken is JObject jObject)
            {
                jObject.Remove("#");
            }
        }

        private void CopyPostOperationBuildUp(JToken parentToken, List<JToken> selectedTokens)
        {
            foreach (JToken selectedToken in selectedTokens)
            {
                if (selectedToken != null)
                {
                    JObject parent = parentToken as JObject;
                    JEnumerable<JToken> copyChildren = selectedToken.Children();
                    if (Context.IsAddOrReplacePropertiesMode())
                    {
                        CopyDescendants(parent, copyChildren);
                    }
                    else
                    {
                        foreach (JProperty property in copyChildren)
                        {
                            parent.Add(property.Name, property.Value);
                        }
                    }
                }
            }
        }

        private static void CopyDescendants(JObject parent, JEnumerable<JToken> children)
        {
            if (parent == null)
            {
                return;
            }

            int i = 0;
            while (i < children.Count())
            {
                JToken token = children.ElementAt(i);
                if (token is JProperty property)
                {
                    if (parent.ContainsKey(property.Name))
                    {
                        CopyDescendants(parent[property.Name] as JObject, property.Children());
                        property.Remove();
                    }
                    else
                    {
                        parent.Add(property.Name, property.Value);
                        i++;
                    }
                }
                else if (token is JObject obj)
                {
                    CopyDescendants(parent, obj.Children());
                    i++;
                }
                else
                {
                    i++;
                }
            }
        }

        private static void AddPostOperationBuildUp(JToken parentToken, List<JToken> tokensToAdd)
        {
            if (tokensToAdd != null)
            {
                foreach (JToken token in tokensToAdd)
                {
                    (parentToken as JObject).Add((token as JProperty).Name, (token as JProperty).Value);
                }
            }
        }

        private void DeletePostOperationBuildUp(JToken parentToken, List<JToken> tokensToDelete)
        {

            foreach (string selectedToken in tokensToDelete)
            {
                JToken tokenToRemove = GetSelectableToken(parentToken, Context).Select(selectedToken);

                if (tokenToRemove != null)
                    tokenToRemove.Ancestors().First().Remove();
            }

        }

        private static void ReplacePostOperationBuildUp(JToken parentToken, Dictionary<string, JToken> tokensToReplace)
        {

            foreach (KeyValuePair<string, JToken> tokenToReplace in tokensToReplace)
            {
                JToken selectedToken = (parentToken as JObject).SelectToken(tokenToReplace.Key);
                selectedToken.Replace(tokenToReplace.Value);
            }

        }

        private static void LoopPostOperationBuildUp(ref JToken parentToken, List<string> condProps, List<string> loopProperties, JArray arrayToForm, JObject dictToForm)
        {
            if (loopProperties != null)
            {
                if (parentToken is JObject obj)
                {
                    foreach (string propertyToDelete in loopProperties)
                    {
                        if (dictToForm == null && arrayToForm == null && parentToken.Count() <= 1)
                        {
                            obj.Replace(JValue.CreateNull());
                        }
                        else
                        {
                            obj.Remove(propertyToDelete);
                        }
                    }
                }
            }

            if (condProps != null)
            {
                if (parentToken is JObject obj)
                {
                    foreach (string propertyToDelete in condProps)
                    {
                        obj.Remove(propertyToDelete);
                    }
                }
            }

            if (dictToForm != null)
            {
                parentToken.Replace(dictToForm);
            }
            else if (arrayToForm != null)
            {
                if (parentToken.Parent != null)
                {
                    if (parentToken.Parent is JArray arr)
                    {
                        foreach (var item in arrayToForm)
                        {
                            arr.Add(item);
                        }
                        if (!parentToken.HasValues)
                        {
                            var tmp = parentToken;
                            parentToken = arr;
                            tmp.Remove();
                        }
                    }
                    else
                    {
                        parentToken.Replace(arrayToForm);
                    }
                }
                else
                {
                    parentToken = arrayToForm;
                }
            }
        }

        private void LoopOperation(string propertyName, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<string> loopProperties, ref JArray arrayToForm, ref JObject dictToForm, JToken childToken)
        {
            var args = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            var previousAlias = "root";
            args[0] = (string)ParseFunction(args[0], parentArray, currentArrayToken);
            string alias = args.Length > 1 ? (string)ParseFunction(args[1].Trim(), parentArray, currentArrayToken) : $"loop{++_loopCounter}";

            if (args.Length > 2)
            {
                previousAlias = (string)ParseFunction(args[2].Trim(), parentArray, currentArrayToken);
                currentArrayToken = new Dictionary<string, JToken> { { previousAlias, Context.Input } };
            }
            else if (currentArrayToken?.Any() ?? false)
            {
                previousAlias = currentArrayToken.Last().Key;
            }
            else
            {
                currentArrayToken = new Dictionary<string, JToken> { { previousAlias, Context.Input } };
            }

            var strArrayToken = ParseArgument(parentArray, currentArrayToken, args[0]) as string;

            bool isDictionary = false;
            JToken arrayToken;
            var selectable = GetSelectableToken(currentArrayToken[previousAlias], Context);
            arrayToken = selectable.Select(strArrayToken);

            if (arrayToken != null)
            {
                //workaround: result should be an array if path ends up with array filter
                if (typeof(T) == typeof(JsonPathSelectable) && arrayToken.Type != JTokenType.Array && (Regex.IsMatch(strArrayToken ?? string.Empty, "\\[.+\\]$") || (currentArrayToken != null && currentArrayToken.ContainsKey(alias) && currentArrayToken[alias] != null && Regex.IsMatch(currentArrayToken[alias].Value<string>(), "\\[.+\\]$"))))
                {
                    arrayToken = new JArray(arrayToken);
                }

                if (arrayToken is IDictionary<string, JToken> dict) //JObject is a dictionary
                {
                    isDictionary = true;
                    JArray arr = new JArray();
                    foreach (var item in dict)
                    {
                        arr.Add(new JObject { { item.Key, item.Value } });
                    }

                    arrayToken = arr;
                }

                if (arrayToken is JArray array)
                {
                    using (IEnumerator<JToken> elements = array.GetEnumerator())
                    {
                        if (parentArray?.Any() ?? false)
                        {
                            parentArray.Add(alias, array);
                        }
                        else
                        {
                            parentArray = new Dictionary<string, JArray> { { alias, array } };
                        }

                        if (arrayToForm == null)
                        {
                            arrayToForm = new JArray();
                        }
                        if (!isDictionary)
                        {
                            while (elements.MoveNext())
                            {
                                JToken clonedToken = childToken.DeepClone();
                                if (currentArrayToken.ContainsKey(alias))
                                {
                                    currentArrayToken[alias] = elements.Current;
                                }
                                else
                                {
                                    currentArrayToken.Add(alias, elements.Current);
                                }
                                RecursiveEvaluate(ref clonedToken, parentArray, currentArrayToken);
                                foreach (JToken replacedProperty in clonedToken.Children())
                                {
                                    arrayToForm.Add(replacedProperty.Type != JTokenType.Null ? replacedProperty : new JObject());
                                }
                            }
                        }
                        else
                        {
                            dictToForm = new JObject();
                            while (elements.MoveNext())
                            {
                                JToken clonedToken = childToken.DeepClone();
                                if (currentArrayToken.ContainsKey(alias))
                                {
                                    currentArrayToken[alias] = elements.Current;
                                }
                                else
                                {
                                    currentArrayToken.Add(alias, elements.Current);
                                }
                                RecursiveEvaluate(ref clonedToken, parentArray, currentArrayToken);
                                foreach (JToken replacedProperty in clonedToken.Children().Select(t => t.First))
                                {
                                    dictToForm.Add(replacedProperty);
                                }
                            }
                        }

                        parentArray.Remove(alias);
                        currentArrayToken.Remove(alias);
                    }
                }
            }

            if (loopProperties == null)
                loopProperties = new List<string>();

            loopProperties.Add(propertyName);
            _loopCounter--;
        }

        private void ConditionalGroupOperation(string propertyName, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<string> condProps, ref List<JToken> tokenToForm, JToken childToken)
        {
            object functionResult = ParseFunction(arguments, parentArray, currentArrayToken);
            bool result;
            try
            {
                result = (bool)ReflectionHelper.GetTypedValue(typeof(bool), functionResult, Context.EvaluationMode);
            }
            catch
            {
                if (Context.IsStrictMode()) { throw; }
                result = false;
            }

            if (result)
            {
                if (condProps == null)
                    condProps = new List<string>();

                condProps.Add(propertyName);

                RecursiveEvaluate(ref childToken, parentArray, currentArrayToken);

                if (tokenToForm == null)
                {
                    tokenToForm = new List<JToken>();
                }

                foreach (JToken grandChildToken in childToken.Children())
                {
                    tokenToForm.Add(grandChildToken.DeepClone());
                }
            }
            else
            {
                if (condProps == null)
                {
                    condProps = new List<string>();
                }

                condProps.Add(propertyName);
            }
        }

        private void EvalOperation(JProperty property, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<string> loopProperties, ref List<JToken> tokensToAdd)
        {
            object functionResult = ParseFunction(arguments, parentArray, currentArrayToken);

            object val;
            if (property.Value.Type == JTokenType.String)
            {
                val = ParseFunction(property.Value.Value<string>(), parentArray, currentArrayToken);
            }
            else
            {
                var propVal = property.Value;
                RecursiveEvaluate(ref propVal, parentArray, currentArrayToken);
                val = property.Value;
            }

            JProperty clonedProperty = new JProperty(functionResult.ToString(), val);

            if (loopProperties == null)
                loopProperties = new List<string>();

            loopProperties.Add(property.Name);

            if (tokensToAdd == null)
            {
                tokensToAdd = new List<JToken>();
            }
            tokensToAdd.Add(clonedProperty);
        }

        private void BulkOperations(JEnumerable<JToken> arrayValues, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, ref List<JToken> selectedTokens, ref Dictionary<string, JToken> tokensToReplace, ref List<JToken> tokensToDelete)
        {
            foreach (JToken arrayValue in arrayValues)
            {
                if (arrayValue.Type == JTokenType.String &&
                    ExpressionHelper.TryParseFunctionNameAndArguments(
                        arrayValue.Value<string>().Trim(), out string functionName, out string arguments))
                {
                    if (functionName == "copy")
                    {
                        if (selectedTokens == null)
                            selectedTokens = new List<JToken>();
                        selectedTokens.Add(Copy(arguments, parentArray, currentArrayToken));
                    }
                    else if (functionName == "replace")
                    {
                        if (tokensToReplace == null)
                            tokensToReplace = new Dictionary<string, JToken>();

                        var replaceResult = Replace(arguments, parentArray, currentArrayToken);
                        tokensToReplace.Add(replaceResult.Key, replaceResult.Value);
                    }
                    else if (functionName == "delete")
                    {
                        if (tokensToDelete == null)
                            tokensToDelete = new List<JToken>();

                        tokensToDelete.Add(Delete(arguments, parentArray, currentArrayToken));
                    }
                }
            }
        }

        private static void BuildArrayToken(JArray arrayToken, IEnumerable<object> itemsToAdd)
        {
            arrayToken.RemoveAll();
            foreach (object itemToAdd in itemsToAdd)
            {
                if (itemToAdd is Array)
                {
                    foreach (var item in itemToAdd as Array)
                    {
                        arrayToken.Add(Utilities.GetNestedData(item));
                    }
                }
                else
                {
                    if (itemToAdd != null)
                    {
                        arrayToken.Add(JToken.FromObject(itemToAdd));
                    }
                }
            }
        }
        #endregion

        private JToken GetToken(object newValue)
        {
            JToken result = null;
            if (newValue != null)
            {
                if (newValue is JToken token)
                {
                    result = token;
                }
                else
                {
                    try
                    {
                        if (newValue is IEnumerable<object> newArray)
                        {
                            result = new JArray(newArray);
                        }
                        else
                        {
                            result = new JValue(newValue);
                        }
                    }
                    catch
                    {
                        if (Context.IsStrictMode())
                        {
                            throw;
                        }

                        if (Context.IsFallbackToDefault())
                        {
                            result = JValue.CreateNull();
                        }
                    }
                }
            }
            else
            {
                result = JValue.CreateNull();
            }

            return result;
        }

        private IEnumerable<object> TransformArray(JEnumerable<JToken> children, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken)
        {
            var result = new List<object>();

            foreach (JToken arrEl in children)
            {
                object itemToAdd = arrEl.Value<JToken>();
                if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                {
                    itemToAdd = ParseFunction(arrEl.ToString(), parentArray, currentArrayToken);
                }
                result.Add(itemToAdd);
            }

            return result;
        }

        #region Copy
        private JToken Copy(string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            string path = argumentArr[0];
            if (!(ParseArgument(parentArray, currentArrayElement, path) is string jsonPath))
            {
                throw new ArgumentException($"Invalid path for #copy: '{argumentArr[0]}' resolved to null!");
            }

            string alias = null;
            if (argumentArr.Length > 1)
            {
                alias = ParseArgument(parentArray, currentArrayElement, argumentArr[1]) as string;
                if (!(currentArrayElement?.ContainsKey(alias) ?? false))
                {
                    throw new ArgumentException($"Unknown loop alias: '{argumentArr[1]}'");
                }
            }
            JToken input = alias != null ? currentArrayElement[alias] : currentArrayElement?.Last().Value ?? Context.Input;
            JToken selectedToken = GetSelectableToken(input, Context).Select(jsonPath);
            return selectedToken;
        }

        #endregion

        #region Replace
        private KeyValuePair<string, JToken> Replace(string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            if (argumentArr.Length < 2)
            {
                throw new Exception("Function #replace needs at least two arguments - 1. path to be replaced, 2. token to replace with.");
            }
            if (!(ParseArgument(parentArray, currentArrayElement, argumentArr[0]) is string key))
            {
                throw new ArgumentException($"Invalid path for #replace: '{argumentArr[0]}' resolved to null!");
            }
            object str = ParseArgument(parentArray, currentArrayElement, argumentArr[1]);
            JToken newToken = GetToken(str);
            return new KeyValuePair<string, JToken>(key, newToken);
        }

        #endregion

        #region Delete
        private string Delete(string argument, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement)
        {
            if (!(ParseArgument(parentArray, currentArrayElement, argument) is string result))
            {
                throw new ArgumentException($"Invalid path for #delete: '{argument}' resolved to null!");
            }
            return result;
        }
        #endregion

        #region ParseFunction

        private object ParseFunction(string functionString, IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement)
        {
            try
            {
                object output = null;
                if (!ExpressionHelper.TryParseFunctionNameAndArguments(functionString, out string functionName, out string argumentString))
                {
                    return functionName;
                }

                string[] arguments = ExpressionHelper.SplitArguments(argumentString, Context.EscapeChar);
                var listParameters = new List<object>();

                if (functionName == "ifcondition")
                {
                    var condition = ParseArgument(array, currentArrayElement, arguments[0]);
                    var value = ParseArgument(array, currentArrayElement, arguments[1]);
                    var equal = ComparisonHelper.Equals(condition, value, Context.EvaluationMode);
                    var index = (equal) ? 2 : 3;

                    output = ParseArgument(array, currentArrayElement, arguments[index]);
                }
                else
                {
                    int i = 0;
                    for (; i < (arguments?.Length ?? 0); i++)
                    {
                        listParameters.Add(ParseArgument(array, currentArrayElement, arguments[i]));
                    }
                    listParameters.Add(Context);

                    var parameters = listParameters.ToArray();
                    var convertParameters = true;
                    if (new[] { "concat", "xconcat", "currentproperty" }.Contains(functionName))
                    {
                        convertParameters = false;
                    }

                    if (new[] { "currentvalue", "currentindex", "lastindex", "lastvalue" }.Contains(functionName))
                    {
                        var alias = ParseLoopAlias(listParameters, 1, array.Last().Key);
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, new object[] { array[alias], currentArrayElement[alias] }, convertParameters, Context);
                    }
                    else if (new[] { "currentvalueatpath", "lastvalueatpath" }.Contains(functionName))
                    {
                        var alias = ParseLoopAlias(listParameters, 2, array.Last().Key);
                        output = ReflectionHelper.Caller<T>(
                            null,
                            "JUST.Transformer`1",
                            functionName,
                            new[] { array[alias], currentArrayElement[alias] }.Concat(listParameters.ToArray()).ToArray(),
                            convertParameters,
                            Context);
                    }
                    else if (functionName == "currentproperty")
                    {
                        var alias = ParseLoopAlias(listParameters, 1, array.Last().Key);
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName,
                            new object[] { array[alias], currentArrayElement[alias], Context },
                            convertParameters, Context);
                    }
                    else if (functionName == "customfunction")
                        output = CallCustomFunction(listParameters.ToArray());
                    else if (Context?.IsRegisteredCustomFunction(functionName) ?? false)
                    {
                        var methodInfo = Context.GetCustomMethod(functionName);
                        output = ReflectionHelper.InvokeCustomMethod<T>(methodInfo, parameters, convertParameters, Context);
                    }
                    else if (Regex.IsMatch(functionName, ReflectionHelper.EXTERNAL_ASSEMBLY_REGEX))
                    {
                        output = ReflectionHelper.CallExternalAssembly<T>(functionName, listParameters.ToArray(), Context);
                    }
                    else if (new[] { "xconcat", "xadd",
                        "mathequals", "mathgreaterthan", "mathlessthan", "mathgreaterthanorequalto", "mathlessthanorequalto",
                        "stringcontains", "stringequals"}.Contains(functionName))
                    {
                        object[] oParams = new object[1];
                        oParams[0] = parameters;
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, oParams, convertParameters, Context);
                    }
                    else if (functionName == "applyover")
                    {
                        output = ParseApplyOver(array, currentArrayElement, parameters);
                    }
                    else
                    {
                        var input = ((JUSTContext)listParameters.Last()).Input;
                        if (currentArrayElement != null && functionName != "valueof")
                        {
                            ((JUSTContext)listParameters.Last()).Input = currentArrayElement.Last().Value;
                        }
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, parameters, convertParameters, Context);
                        ((JUSTContext)parameters.Last()).Input = input;
                    }
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling function : " + functionString + " - " + ex.Message, ex);
            }
        }

        private object ParseApplyOver(IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, object[] parameters)
        {
            object output;
            var contextInput = Context.Input;
            var input = JToken.Parse(Transform(parameters[0].ToString(), contextInput.ToString()));
            Context.Input = input;
            if (parameters[1].ToString().Trim().Trim('\'').StartsWith("{"))
            {
                var jobj = JObject.Parse(parameters[1].ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jobj, input);
            }
            else if (parameters[1].ToString().Trim().Trim('\'').StartsWith("["))
            {
                var jarr = JArray.Parse(parameters[1].ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jarr, input);
            }
            else
            {
                output = ParseFunction(parameters[1].ToString().Trim().Trim('\''), array, currentArrayElement);
            }
            Context.Input = contextInput;
            return output;
        }

        private string ParseLoopAlias(List<object> listParameters, int index, string defaultValue)
        {
            string alias;
            if (listParameters.Count > index)
            {
                alias = (listParameters[index - 1] as string).Trim();
                listParameters.RemoveAt(index - 1);
            }
            else
            {
                alias = defaultValue; //$"loop{_loopCounter}";
            }
            return alias;
        }

        private object ParseArgument(IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, string argument)
        {
            var trimmedArgument = argument.Trim();
            if (trimmedArgument.StartsWith("#"))
            {
                return ParseFunction(trimmedArgument, array, currentArrayElement);
            }
            if (trimmedArgument.StartsWith($"{Context.EscapeChar}#"))
            {
                return ExpressionHelper.UnescapeSharp(argument, Context.EscapeChar);
            }
            return argument;
        }

        private object CallCustomFunction(object[] parameters)
        {
            object[] customParameters = new object[parameters.Length - 3];
            string functionString = string.Empty;
            string dllName = string.Empty;
            int i = 0;
            foreach (object parameter in parameters)
            {
                if (i == 0)
                    dllName = parameter.ToString();
                else if (i == 1)
                    functionString = parameter.ToString();
                else
                if (i != (parameters.Length - 1))
                    customParameters[i - 2] = parameter;

                i++;
            }

            int index = functionString.LastIndexOf(".");

            string className = functionString.Substring(0, index);
            string functionName = functionString.Substring(index + 1, functionString.Length - index - 1);

            className = className + "," + dllName;

            return ReflectionHelper.Caller<T>(null, className, functionName, customParameters, true, Context);

        }
        #endregion

        #region Split
        public static IEnumerable<string> SplitJson(string input, string arrayPath, JUSTContext context)
        {
            JObject inputJObject = DeserializeWithoutDateParse<JObject>(input);

            List<JObject> jObjects = SplitJson(inputJObject, arrayPath, context).ToList();

            List<string> output = null;

            foreach (JObject jObject in jObjects)
            {
                if (output == null)
                    output = new List<string>();

                output.Add(SerializeWithoutDateParse(jObject));
            }

            return output;
        }

        public static IEnumerable<JObject> SplitJson(JObject input, string arrayPath, JUSTContext context)
        {
            List<JObject> jsonObjects = null;

            JToken tokenArr = GetSelectableToken(input, context).Select(arrayPath);

            string pathToReplace = tokenArr.Path;

            if (tokenArr != null && tokenArr is JArray)
            {
                JArray array = tokenArr as JArray;

                foreach (JToken tokenInd in array)
                {

                    string path = tokenInd.Path;

                    JToken clonedToken = input.DeepClone();

                    var selectable = GetSelectableToken(clonedToken, context);
                    JToken foundToken = selectable.Select(selectable.RootReference + path);
                    JToken tokenToReplce = selectable.Select(selectable.RootReference + pathToReplace);

                    tokenToReplce.Replace(foundToken);

                    if (jsonObjects == null)
                        jsonObjects = new List<JObject>();

                    jsonObjects.Add(clonedToken as JObject);


                }
            }
            else
                throw new Exception("ArrayPath must be a valid JSON path to a JSON array.");

            return jsonObjects;
        }
        #endregion

        private static int GetIndexOfFunctionEnd(string totalString)
        {
            int index = -1;

            int startIndex = totalString.IndexOf("#");

            int startBrackettCount = 0;
            int endBrackettCount = 0;

            for (int i = startIndex; i < totalString.Length; i++)
            {
                if (totalString[i] == '(')
                    startBrackettCount++;
                if (totalString[i] == ')')
                    endBrackettCount++;

                if (endBrackettCount == startBrackettCount && endBrackettCount > 0)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static T GetSelectableToken(JToken token, JUSTContext context)
        {
            return context.Resolve<T>(token);
        }
    }
}
