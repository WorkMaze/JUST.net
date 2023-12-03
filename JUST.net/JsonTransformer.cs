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

        private static string SerializeWithoutDateParse<U>(U obj)
        {
            var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
            return JsonConvert.SerializeObject(obj, settings);
        }

        private static U DeserializeWithoutDateParse<U>(string inputJson)
        {
            var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
            return JsonConvert.DeserializeObject<U>(inputJson, settings);
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
            var parentToken = (JToken)transformer;
            RecursiveEvaluate(ref parentToken, null, null, input);
            return parentToken;
        }

        #region RecursiveEvaluate


        private void RecursiveEvaluate(ref JToken parentToken, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, JToken input)
        {
            if (parentToken == null)
            {
                return;
            }

            JEnumerable<JToken> tokens = parentToken.Children();

            TransformHelper helper = new TransformHelper();
            foreach (JToken childToken in tokens)
            {
                ParseToken(parentToken, parentArray, currentArrayToken, helper, childToken, input);
            }

            if (helper.selectedTokens != null)
            {
                CopyPostOperationBuildUp(parentToken, helper.selectedTokens);
            }
            if (helper.tokensToReplace != null)
            {
                ReplacePostOperationBuildUp(parentToken, helper.tokensToReplace);
            }
            if (helper.tokensToDelete != null)
            {
                DeletePostOperationBuildUp(parentToken, helper.tokensToDelete);
            }
            if (helper.tokensToAdd != null)
            {
                AddPostOperationBuildUp(parentToken, helper.tokensToAdd);
            }
            PostOperationsBuildUp(ref parentToken, helper.tokenToForm);
            if (helper.loopProperties != null || helper.condProps != null)
            {
                LoopPostOperationBuildUp(ref parentToken, helper);
            }
        }

        private void ParseToken(JToken parentToken, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken childToken, JToken input)
        {
            if (childToken.Type == JTokenType.Array && (parentToken as JProperty)?.Name.Trim() != "#")
            {
                IEnumerable<object> itemsToAdd = TransformArray(childToken.Children(), parentArray, currentArrayToken, input);
                BuildArrayToken(childToken as JArray, itemsToAdd);
            }
            else if (childToken.Type == JTokenType.Property && childToken is JProperty property && property.Name != null)
            {
                /* For looping*/
                helper.isLoop = false;

                if (property.Name == "#" && property.Value.Type == JTokenType.Array && property.Value is JArray values)
                {
                    BulkOperations(values.Children(), parentArray, currentArrayToken, helper, input);
                    helper.isBulk = true;
                }
                else
                {
                    helper.isBulk = false;
                    if (ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments))
                    {
                        ParsePropertyFunction(parentArray, currentArrayToken, helper, childToken, property, functionName, arguments, input);
                    }
                    else if (property.Value.ToString().Trim().StartsWith("#"))
                    {
                        var propVal = property.Value.ToString().Trim();
                        var output = ParseFunction(propVal, parentToken, parentArray, currentArrayToken, input);
                        output = LookInTransformed(output, propVal, parentToken, parentArray, currentArrayToken, input);
                        property.Value = GetToken(output);
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
                object newValue = ParseFunction(childToken.Value<string>(), parentToken, parentArray, currentArrayToken, input);
                childToken.Replace(GetToken(newValue));
            }

            if (!helper.isLoop && !helper.isBulk)
            {
                RecursiveEvaluate(ref childToken, parentArray, currentArrayToken, input);
            }
        }

        private void ParsePropertyFunction(IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken childToken, JProperty property, string functionName, string arguments, JToken input)
        {
            switch (functionName)
            {
                case "ifgroup":
                    ConditionalGroupOperation(property.Name, arguments, parentArray, currentArrayToken, helper, childToken, input);
                    break;
                case "loop":
                    LoopOperation(property.Name, arguments, parentArray, currentArrayToken, helper, childToken, input);
                    helper.isLoop = true;
                    break;
                case "eval":
                    EvalOperation(property, arguments, parentArray, currentArrayToken, helper, input);
                    break;
            }
        }

        private void PostOperationsBuildUp(ref JToken parentToken, IList<JToken> tokenToForm)
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

        private void CopyPostOperationBuildUp(JToken parentToken, IList<JToken> selectedTokens)
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

        private static void AddPostOperationBuildUp(JToken parentToken, IList<JToken> tokensToAdd)
        {
            if (tokensToAdd != null)
            {
                foreach (JToken token in tokensToAdd)
                {
                    (parentToken as JObject).Add((token as JProperty).Name, (token as JProperty).Value);
                }
            }
        }

        private void DeletePostOperationBuildUp(JToken parentToken, IList<JToken> tokensToDelete)
        {

            foreach (string selectedToken in tokensToDelete)
            {
                JToken tokenToRemove = GetSelectableToken(parentToken, Context).Select(selectedToken);

                if (tokenToRemove != null)
                    tokenToRemove.Ancestors().First().Remove();
            }
        }

        private void ReplacePostOperationBuildUp(JToken parentToken, IDictionary<string, JToken> tokensToReplace)
        {

            foreach (KeyValuePair<string, JToken> tokenToReplace in tokensToReplace)
            {
                JsonPathSelectable selectable = JsonTransformer.GetSelectableToken(parentToken, Context);
                JToken selectedToken = selectable.Select(tokenToReplace.Key);
                selectedToken.Replace(tokenToReplace.Value);
            }
        }

        private static void LoopPostOperationBuildUp(ref JToken parentToken, TransformHelper helper)
        {
            if (helper.loopProperties != null)
            {
                if (parentToken is JObject obj)
                {
                    foreach (string propertyToDelete in helper.loopProperties)
                    {
                        if (helper.dictToForm == null && helper.arrayToForm == null && parentToken.Count() <= 1)
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

            if (helper.condProps != null)
            {
                if (parentToken is JObject obj)
                {
                    foreach (string propertyToDelete in helper.condProps)
                    {
                        obj.Remove(propertyToDelete);
                    }
                }
            }

            if (helper.dictToForm != null)
            {
                parentToken.Replace(helper.dictToForm);
            }
            else if (helper.arrayToForm != null)
            {
                if (parentToken.Parent != null)
                {
                    if (parentToken.Parent is JArray arr)
                    {
                        foreach (var item in helper.arrayToForm)
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
                        parentToken.Replace(helper.arrayToForm);
                    }
                }
                else
                {
                    parentToken = helper.arrayToForm;
                }
            }
        }

        private void LoopOperation(string propertyName, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken childToken, JToken input)
        {
            var args = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            var previousAlias = "root";
            args[0] = (string)ParseFunction(args[0], null, parentArray, currentArrayToken, input);
            string alias = args.Length > 1 ? (string)ParseFunction(args[1].Trim(), null, parentArray, currentArrayToken, input) : $"loop{++_loopCounter}";

            if (args.Length > 2)
            {
                previousAlias = (string)ParseFunction(args[2].Trim(), null, parentArray, currentArrayToken, input);
                currentArrayToken = new Dictionary<string, JToken> { { previousAlias, input } };
            }
            else if (currentArrayToken?.Any() ?? false)
            {
                previousAlias = currentArrayToken.Last().Key;
            }
            else
            {
                currentArrayToken = new Dictionary<string, JToken> { { previousAlias, input } };
            }

            var strArrayToken = ParseArgument(null, parentArray, currentArrayToken, args[0], input) as string;

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

                        if (helper.arrayToForm == null)
                        {
                            helper.arrayToForm = new JArray();
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
                                RecursiveEvaluate(ref clonedToken, parentArray, currentArrayToken, input);
                                foreach (JToken replacedProperty in clonedToken.Children())
                                {
                                    helper.arrayToForm.Add(replacedProperty.Type != JTokenType.Null ? replacedProperty : new JObject());
                                }
                            }
                        }
                        else
                        {
                            helper.dictToForm = new JObject();
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
                                RecursiveEvaluate(ref clonedToken, parentArray, currentArrayToken, input);
                                foreach (JToken replacedProperty in clonedToken.Children().Select(t => t.First))
                                {
                                    helper.dictToForm.Add(replacedProperty);
                                }
                            }
                        }

                        parentArray.Remove(alias);
                        currentArrayToken.Remove(alias);
                    }
                }
            }

            if (helper.loopProperties == null)
                helper.loopProperties = new List<string>();

            helper.loopProperties.Add(propertyName);
            _loopCounter--;
        }

        private void ConditionalGroupOperation(string propertyName, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken childToken, JToken input)
        {
            object functionResult = ParseFunction(arguments, null, parentArray, currentArrayToken, input);
            bool result;
            try
            {
                result = (bool)ReflectionHelper.GetTypedValue(typeof(bool), functionResult, Context.IsStrictMode());
            }
            catch
            {
                if (Context.IsStrictMode()) { throw; }
                result = false;
            }

            if (result)
            {
                if (helper.condProps == null)
                    helper.condProps = new List<string>();

                helper.condProps.Add(propertyName);

                RecursiveEvaluate(ref childToken, parentArray, currentArrayToken, input);

                if (helper.tokenToForm == null)
                {
                    helper.tokenToForm = new List<JToken>();
                }

                foreach (JToken grandChildToken in childToken.Children())
                {
                    helper.tokenToForm.Add(grandChildToken.DeepClone());
                }
            }
            else
            {
                if (helper.condProps == null)
                {
                    helper.condProps = new List<string>();
                }

                helper.condProps.Add(propertyName);
                childToken.First.Replace(JToken.Parse("{}"));
            }
        }

        private void EvalOperation(JProperty property, string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken input)
        {
            object functionResult = ParseFunction(arguments, null, parentArray, currentArrayToken, input);

            object val;
            if (property.Value.Type == JTokenType.String)
            {
                val = ParseFunction(property.Value.Value<string>(), null, parentArray, currentArrayToken, input);
            }
            else
            {
                var propVal = property.Value;
                RecursiveEvaluate(ref propVal, parentArray, currentArrayToken, input);
                val = property.Value;
            }

            JProperty clonedProperty = new JProperty(functionResult.ToString(), val);

            if (helper.loopProperties == null)
                helper.loopProperties = new List<string>();

            helper.loopProperties.Add(property.Name);

            if (helper.tokensToAdd == null)
            {
                helper.tokensToAdd = new List<JToken>();
            }
            helper.tokensToAdd.Add(clonedProperty);
        }

        private void BulkOperations(JEnumerable<JToken> arrayValues, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, TransformHelper helper, JToken input)
        {
            foreach (JToken arrayValue in arrayValues)
            {
                if (arrayValue.Type == JTokenType.String &&
                    ExpressionHelper.TryParseFunctionNameAndArguments(
                        arrayValue.Value<string>().Trim(), out string functionName, out string arguments))
                {
                    if (functionName == "copy")
                    {
                        if (helper.selectedTokens == null)
                            helper.selectedTokens = new List<JToken>();
                        helper.selectedTokens.Add(Copy(arguments, parentArray, currentArrayToken, input));
                    }
                    else if (functionName == "replace")
                    {
                        if (helper.tokensToReplace == null)
                            helper.tokensToReplace = new Dictionary<string, JToken>();

                        var replaceResult = Replace(arguments, parentArray, currentArrayToken, input);
                        helper.tokensToReplace.Add(replaceResult.Key, replaceResult.Value);
                    }
                    else if (functionName == "delete")
                    {
                        if (helper.tokensToDelete == null)
                            helper.tokensToDelete = new List<JToken>();

                        helper.tokensToDelete.Add(Delete(arguments, parentArray, currentArrayToken, input));
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

        private IEnumerable<object> TransformArray(JEnumerable<JToken> children, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, JToken input)
        {
            var result = new List<object>();

            foreach (JToken arrEl in children)
            {
                object itemToAdd = arrEl.Value<JToken>();
                if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                {
                    itemToAdd = ParseFunction(arrEl.ToString(), null, parentArray, currentArrayToken, input);
                }
                result.Add(itemToAdd);
            }

            return result;
        }

        #region Copy
        private JToken Copy(string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement, JToken input)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            string path = argumentArr[0];
            if (!(ParseArgument(null, parentArray, currentArrayElement, path, input) is string jsonPath))
            {
                throw new ArgumentException($"Invalid path for #copy: '{argumentArr[0]}' resolved to null!");
            }

            string alias = null;
            if (argumentArr.Length > 1)
            {
                alias = ParseArgument(null, parentArray, currentArrayElement, argumentArr[1], input) as string;
                if (!(currentArrayElement?.ContainsKey(alias) ?? false))
                {
                    throw new ArgumentException($"Unknown loop alias: '{argumentArr[1]}'");
                }
            }
            JToken localInput = alias != null ? currentArrayElement[alias] : currentArrayElement?.Last().Value ?? input;
            JToken selectedToken = GetSelectableToken(localInput, Context).Select(jsonPath);
            return selectedToken;
        }

        #endregion

        #region Replace
        private KeyValuePair<string, JToken> Replace(string arguments, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement, JToken input)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            if (argumentArr.Length < 2)
            {
                throw new Exception("Function #replace needs at least two arguments - 1. path to be replaced, 2. token to replace with.");
            }
            if (!(ParseArgument(null, parentArray, currentArrayElement, argumentArr[0], input) is string key))
            {
                throw new ArgumentException($"Invalid path for #replace: '{argumentArr[0]}' resolved to null!");
            }
            object str = ParseArgument(null, parentArray, currentArrayElement, argumentArr[1], input);
            JToken newToken = GetToken(str);
            return new KeyValuePair<string, JToken>(key, newToken);
        }

        #endregion

        #region Delete
        private string Delete(string argument, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayElement, JToken input)
        {
            if (!(ParseArgument(null, parentArray, currentArrayElement, argument, input) is string result))
            {
                throw new ArgumentException($"Invalid path for #delete: '{argument}' resolved to null!");
            }
            return result;
        }
        #endregion

        #region ParseFunction

        private object ParseFunction(string functionString, JToken parentToken, IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, JToken input)
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
                    output = GetConditionalOutput(parentToken, arguments, array, currentArrayElement, input);
                }
                else
                {
                    int i = 0;
                    for (; i < (arguments?.Length ?? 0); i++)
                    {
                        output = ParseArgument(parentToken, array, currentArrayElement, arguments[i], input);
                        output = LookInTransformed(output, arguments[i], parentToken, array, currentArrayElement, input);
                        listParameters.Add(output);
                    }

                    var convertParameters = true;
                    if (new[] { "concat", "xconcat", "currentproperty" }.Contains(functionName))
                    {
                        convertParameters = false;
                    }

                    output = GetFunctionOutput(functionName, listParameters, convertParameters, array, currentArrayElement, input);
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling function : " + functionString + " - " + ex.Message, ex);
            }
        }

        private object ConditionalFunction(IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, string[] arguments, JToken input)
        {
            var condition = ParseArgument(null, array, currentArrayElement, arguments[0], input);
            var value = ParseArgument(null, array, currentArrayElement, arguments[1], input);
            var equal = ComparisonHelper.Equals(condition, value, Context.EvaluationMode);
            var index = equal ? 2 : 3;

            return ParseArgument(null, array, currentArrayElement, arguments[index], input);
        }

        private object ParseApplyOver(IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, object[] parameters, JToken input)
        {
            object output;
            string localInput = Transform(parameters[0].ToString(), input.ToString());
            if (parameters[1].ToString().Trim().Trim('\'').StartsWith("{"))
            {
                var jobj = JObject.Parse(parameters[1].ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jobj, localInput);
            }
            else if (parameters[1].ToString().Trim().Trim('\'').StartsWith("["))
            {
                var jarr = JArray.Parse(parameters[1].ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jarr, localInput);
            }
            else
            {
                output = ParseFunction(parameters[1].ToString().Trim().Trim('\''), null, array, currentArrayElement, JToken.Parse(localInput));
            }
            return output;
        }

        private string ParseLoopAlias(IList<object> listParameters, int index, string defaultValue)
        {
            string alias;
            if (listParameters != null && listParameters.Count >= index)
            {
                alias = (listParameters[index - 1] as string).Trim();
                listParameters.RemoveAt(index - 1);
            }
            else
            {
                alias = defaultValue;
            }
            return alias;
        }

        private object ParseArgument(JToken parentToken, IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, string argument, JToken input)
        {
            object output = argument;
            var trimmedArgument = argument.Trim();
            if (trimmedArgument.StartsWith("#"))
            {
                output = ParseFunction(trimmedArgument, parentToken, array, currentArrayElement, input);
            }
            else if (trimmedArgument.StartsWith($"{Context.EscapeChar}#"))
            {
                output = ExpressionHelper.UnescapeSharp(argument, Context.EscapeChar);
            }
            return output;
        }

        private object GetConditionalOutput(JToken parentToken, string[] arguments, IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, JToken input)
        {
            var condition = ParseArgument(parentToken, array, currentArrayElement, arguments[0], input);
            condition = LookInTransformed(condition, arguments[0], parentToken, array, currentArrayElement, input);
            var value = ParseArgument(parentToken, array, currentArrayElement, arguments[1], input);
            value = LookInTransformed(value, arguments[1], parentToken, array, currentArrayElement, input);
            var equal = ComparisonHelper.Equals(condition, value, Context.EvaluationMode);
            var index = equal ? 2 : 3;

            return ParseArgument(parentToken, array, currentArrayElement, arguments[index], input);
        }

        private object GetFunctionOutput(string functionName, IList<object> listParameters, bool convertParameters, IDictionary<string, JArray> array, IDictionary<string, JToken> currentArrayElement, JToken input)
        {
            object output;
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
                    new[] { array[alias], currentArrayElement[alias] }.Concat(new object[] { listParameters[0], Context }).ToArray(),
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
                output = CallCustomFunction(listParameters.Concat(new object[] { currentArrayElement?.Last().Value ?? input, Context }).ToArray());
            else if (Context?.IsRegisteredCustomFunction(functionName) ?? false)
            {
                var methodInfo = Context.GetCustomMethod(functionName);
                output = ReflectionHelper.InvokeCustomMethod<T>(methodInfo, listParameters.ToArray(), convertParameters, Context);
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
                oParams[0] = listParameters.Concat(new object[] { Context }).ToArray();
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, oParams, convertParameters, Context);
            }
            else if (functionName == "applyover")
            {
                output = ParseApplyOver(array, currentArrayElement, listParameters.ToArray(), input);
            }
            else
            {
                var inputToken = input;
                if (currentArrayElement != null && functionName != "valueof")
                {
                    inputToken = currentArrayElement.Last().Value;
                }
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, listParameters.Concat(new object[] { inputToken, Context }).ToArray(), convertParameters, Context);
            }
            return output;
        }

        private object LookInTransformed(object output, string propVal, JToken parentToken, IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, JToken input)
        {
            if (output == null && Context.IsLookInTransformed())
            {
                input = parentToken;
                output = ParseFunction(propVal, parentToken, parentArray, currentArrayToken, input);
            }
            return output;
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
