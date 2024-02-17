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
                { State.RootKey, transformer }
            };
            Transform(tmp, input);
            return tmp[State.RootKey];
        }

        public JToken Transform(JObject transformer, string input)
        {
            return Transform(transformer, DeserializeWithoutDateParse<JToken>(input));
        }

        public JToken Transform(JObject transformer, JToken input)
        {
            Context.Input = input;
            var parentToken = (JToken)transformer;
            State state = new State(transformer, Context.Input, _levelCounter);
            RecursiveEvaluate(ref parentToken, state);
            return parentToken;
        }

        #region RecursiveEvaluate


        private void RecursiveEvaluate(ref JToken parentToken, State state)
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
            List<string> scopeProperties = null;
            List<string> condProps = null;
            JArray arrayToForm = null;
            JObject dictToForm = null;
            JObject scopeToForm = null;
            List<JToken> tokenToForm = null;
            List<JToken> tokensToAdd = null;

            bool isLoop = false;
            bool isBulk = false;
            bool isScope = false;

            foreach (JToken childToken in tokens)
            {
                ParseToken(ref parentToken, state, ref selectedTokens, ref tokensToReplace, ref tokensToDelete, ref loopProperties, ref condProps, ref arrayToForm, ref dictToForm, ref scopeProperties, ref scopeToForm, ref tokenToForm, ref tokensToAdd, ref isLoop, ref isBulk, ref isScope, childToken);
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
            if (scopeToForm != null)
            {
                ScopePostOperationBuildUp(ref parentToken, scopeProperties, scopeToForm);
            }
        }

        private void ParseToken(ref JToken parentToken, State state,
            ref List<JToken> selectedTokens, ref Dictionary<string, JToken> tokensToReplace, ref List<JToken> tokensToDelete,
            ref List<string> loopProperties, ref List<string> condProps, ref JArray arrayToForm, ref JObject dictToForm,
            ref List<string> scopeProperties, ref JObject scopeToForm,
            ref List<JToken> tokenToForm, ref List<JToken> tokensToAdd,
            ref bool isLoop, ref bool isBulk, ref bool isScope, JToken childToken)
        {
            if (childToken.Type == JTokenType.Array && (parentToken as JProperty)?.Name.Trim() != "#")
            {
                IEnumerable<object> itemsToAdd = TransformArray(childToken.Children(), state);
                BuildArrayToken(childToken as JArray, itemsToAdd);
            }
            else if (childToken.Type == JTokenType.Property && childToken is JProperty property && property.Name != null)
            {
                /* For looping*/
                isLoop = false;

                if (property.Name == "#" && property.Value.Type == JTokenType.Array && property.Value is JArray values)
                {
                    BulkOperations(values.Children(), state, ref selectedTokens, ref tokensToReplace, ref tokensToDelete);
                    isBulk = true;
                }
                else
                {
                    isBulk = false;
                    if (ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments))
                    {
                        ParsePropertyFunction(state, ref loopProperties, ref condProps, ref arrayToForm, ref dictToForm, ref scopeProperties, ref scopeToForm, ref tokenToForm, ref tokensToAdd, ref isLoop, ref isScope, childToken, property, functionName, arguments);
                    }
                    else if (property.Value.ToString().Trim().StartsWith("#"))
                    {
                        var propVal = property.Value.ToString().Trim();
                        var output = ParseFunction(propVal, state);
                        output = LookInTransformed(output, propVal, state);
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
                && state.ParentArray != null && state.CurrentArrayToken != null)
            {
                object newValue = ParseFunction(childToken.Value<string>(), state);
                childToken.Replace(GetToken(newValue));
            }

            if (!isLoop && !isBulk && !isScope)
            {
                RecursiveEvaluate(ref childToken, state);
            }
        }

        private void ParsePropertyFunction(State state, ref List<string> loopProperties, ref List<string> condProps, ref JArray arrayToForm, ref JObject dictToForm, ref List<string> scopeProperties, ref JObject scopeToForm, ref List<JToken> tokenToForm, ref List<JToken> tokensToAdd, ref bool isLoop, ref bool isScope, JToken childToken, JProperty property, string functionName, string arguments)
        {
            switch (functionName)
            {
                case "ifgroup":
                    ConditionalGroupOperation(property.Name, arguments, state, ref condProps, ref tokenToForm, childToken);
                    break;
                case "loop":
                    LoopOperation(property.Name, arguments, state, ref loopProperties, ref arrayToForm, ref dictToForm, childToken);
                    isLoop = true;
                    break;
                case "eval":
                    EvalOperation(property, arguments, state, ref loopProperties, ref tokensToAdd);
                    break;
                case "scope":
                    ScopeOperation(property.Name, arguments, state, ref scopeProperties, ref scopeToForm, childToken);
                    isScope = true;
                    break;
                case "transform":
                    TranformOperation(property, arguments, state);
                    break;
            }
        }

        private void TranformOperation(JProperty property, string arguments, State state)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);

            object functionResult = ParseArgument(state, argumentArr[0]);
            if (!(functionResult is string jsonPath))
            {
                throw new ArgumentException($"Invalid path for #transform: '{argumentArr[0]}' resolved to null!");
            }

            JToken selectedToken = null;
            string alias = null;
            if (argumentArr.Length > 1)
            {
                alias = ParseArgument(state, argumentArr[1]) as string;
                if (!(state.CurrentArrayToken?.Any(t => t.Key.Key == alias) ?? false))
                {
                    throw new ArgumentException($"Unknown loop alias: '{argumentArr[1]}'");
                }
                JToken input = alias != null ? state.CurrentArrayToken?.Single(t => t.Key.Key == alias).Value : state.CurrentArrayToken?.Last().Value ?? Context.Input;
                var selectable = GetSelectableToken(state.CurrentArrayToken.Single(t => t.Key.Key == alias).Value, Context);
                selectedToken = selectable.Select(argumentArr[0]);
            }
            else
            {
                var selectable = GetSelectableToken(state.CurrentArrayToken?.Last().Value ?? Context.Input, Context);
                selectedToken = selectable.Select(argumentArr[0]);
            }
            
            if (property.Value.Type == JTokenType.Array)
            {
                JToken originalInput = Context.Input;
                Context.Input = selectedToken;
                for (int i = 0; i < property.Value.Count(); i++)
                {
                    JToken token = property.Value[i];
                    if (token.Type == JTokenType.String)
                    {
                        var obj = ParseFunction(
                            token.Value<string>(),
                            new State(token, Context.Input, _levelCounter,
                                state.CurrentArrayToken.Where(t => t.Key.Key != State.RootKey)
                                    .ToDictionary(p => p.Key, p => p.Value),
                                state.CurrentScopeToken.Where(t => t.Key.Key != State.RootKey)
                                    .ToDictionary(p => p.Key, p => p.Value)));
                        token.Replace(GetToken(obj));
                    }
                    else
                    {
                        RecursiveEvaluate(
                            ref token,
                            new State(token, Context.Input, _levelCounter,
                                state.CurrentArrayToken.Where(t => t.Key.Key != State.RootKey)
                                    .ToDictionary(p => p.Key, p => p.Value),
                                state.CurrentScopeToken.Where(t => t.Key.Key != State.RootKey)
                                    .ToDictionary(p => p.Key, p => p.Value)));
                    }
                    Context.Input = token;
                }
                
                Context.Input = originalInput;
            }
            property.Parent.Replace(property.Value[property.Value.Count() - 1]);
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

        private static void ScopePostOperationBuildUp(ref JToken parentToken, List<string> scopeProperties, JObject scopeToForm)
        {
            if (parentToken is JObject obj)
            {
                foreach (string propertyToDelete in scopeProperties)
                {
                    if (scopeToForm == null && parentToken.Count() <= 1)
                    {
                        obj.Replace(JValue.CreateNull());
                    }
                    else
                    {
                        obj.Remove(propertyToDelete);
                    }
                }
            }

            if (scopeToForm != null)
            {
                parentToken.Replace(scopeToForm);
            }
        }

        private void LoopOperation(string propertyName, string arguments, State state, ref List<string> loopProperties, ref JArray arrayToForm, ref JObject dictToForm, JToken childToken)
        {
            var args = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            var previousAlias = State.RootKey;
            args[0] = (string)ParseFunction(args[0], state);
            _levelCounter++;
            string alias = args.Length > 1 ? (string)ParseFunction(args[1].Trim(), state) : $"loop{_levelCounter}";

            if (args.Length > 2)
            {
                previousAlias = (string)ParseFunction(args[2].Trim(), state);
                state.CurrentArrayToken.Add(new LevelKey() { Key = alias, Level = _levelCounter }, Context.Input);
            }
            else if (state.CurrentArrayToken.Any(t => t.Key.Key == alias))
            {
                previousAlias = state.CurrentArrayToken.Single(t => t.Key.Key == alias).Key.Key;

            }
            else if (state.CurrentScopeToken.Any(t => t.Key.Key == alias))
            {
                previousAlias = state.CurrentScopeToken.Single(t => t.Key.Key == alias).Key.Key;
            }
            else
            {
                previousAlias = state.GetHigherAlias();
            }
            
            var strArrayToken = ParseArgument(state, args[0]) as string;

            bool isDictionary = false;
            JToken arrayToken;
            var selectable = GetSelectableToken(state.GetAliasToken(previousAlias), Context);
            arrayToken = selectable.Select(strArrayToken);

            if (arrayToken != null)
            {
                //workaround: result should be an array if path ends up with array filter
                if (IsArray(arrayToken, strArrayToken, state, alias))
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
                        state.ParentArray.Add(new LevelKey { Level = _levelCounter, Key = alias}, array);
                        
                        if (arrayToForm == null)
                        {
                            arrayToForm = new JArray();
                        }
                        if (!isDictionary)
                        {
                            while (elements.MoveNext())
                            {
                                JToken clonedToken = childToken.DeepClone();
                                if (state.CurrentArrayToken.Any(a => a.Key.Key == alias))
                                {
                                    state.CurrentArrayToken.Remove(new LevelKey { Level = _levelCounter, Key = alias});
                                }
                                state.CurrentArrayToken.Add(new LevelKey { Level = _levelCounter, Key = alias}, elements.Current);
                                RecursiveEvaluate(ref clonedToken, state);
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
                                if (state.CurrentArrayToken.Any(a => a.Key.Key == alias))
                                {
                                    state.CurrentArrayToken.Remove(new LevelKey { Level = _levelCounter, Key = alias});
                                }
                                state.CurrentArrayToken.Add(new LevelKey { Level = _levelCounter, Key = alias}, elements.Current);
                                RecursiveEvaluate(ref clonedToken, state);
                                foreach (JToken replacedProperty in clonedToken.Children().Select(t => t.First))
                                {
                                    dictToForm.Add(replacedProperty);
                                }
                            }
                        }

                        state.ParentArray.Remove(new LevelKey { Level = _levelCounter, Key = alias});
                        state.CurrentArrayToken.Remove(new LevelKey { Level = _levelCounter, Key = alias});
                    }
                }
            }

            if (loopProperties == null)
                loopProperties = new List<string>();

            loopProperties.Add(propertyName);
            _levelCounter--;
        }

        private bool IsArray(JToken arrayToken, string strArrayToken, State state, string alias)
        {
            return typeof(T) == typeof(JsonPathSelectable) && arrayToken.Type != JTokenType.Array && (Regex.IsMatch(strArrayToken ?? string.Empty, "\\[.+\\]$") || (state.CurrentArrayToken != null && state.CurrentArrayToken.Any(a => a.Key.Key == alias) && state.CurrentArrayToken.Single(a => a.Key.Key == alias).Value != null && Regex.IsMatch(state.CurrentArrayToken.Single(a => a.Key.Key == alias).Value.Value<string>(), "\\[.+\\]$")));
        }

        private void ScopeOperation(string propertyName, string arguments, State state, ref List<string> scopeProperties, ref JObject scopeToForm, JToken childToken)
        {
            var args = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            var previousAlias = State.RootKey;
            args[0] = (string)ParseFunction(args[0], state);
            _levelCounter++;
            string alias = args.Length > 1 ? (string)ParseFunction(args[1].Trim(), state) : $"scope{_levelCounter}";

            if (args.Length > 2)
            {
                previousAlias = (string)ParseFunction(args[2].Trim(), state);
            }
            else
            {
                previousAlias = state.GetHigherAlias();
            }

            var strScopeToken = ParseArgument(state, args[0]) as string;

            JToken scopeToken;
            var selectable = GetSelectableToken(state.GetAliasToken(previousAlias), Context);
            scopeToken = selectable.Select(strScopeToken);

            JToken clonedToken = childToken.DeepClone();
            if (state.CurrentScopeToken.Any(s => s.Key.Key == alias))
            {
                state.CurrentScopeToken.Remove(new LevelKey {Level = _levelCounter, Key = alias});
            }
            state.CurrentScopeToken.Add(new LevelKey {Level = _levelCounter, Key = alias}, scopeToken);
            RecursiveEvaluate(ref clonedToken, state);
            scopeToForm = clonedToken.Children().First().Value<JObject>();
            
            state.CurrentScopeToken.Remove(new LevelKey {Level = _levelCounter, Key = alias});

            if (scopeProperties == null)
                scopeProperties = new List<string>();

            scopeProperties.Add(propertyName);
            _levelCounter--;
        }

        private void ConditionalGroupOperation(string propertyName, string arguments, State state, ref List<string> condProps, ref List<JToken> tokenToForm, JToken childToken)
        {
            object functionResult = ParseFunction(arguments, state);
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

                RecursiveEvaluate(ref childToken, state);

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
                childToken.First.Replace(JToken.Parse("{}"));
            }
        }

        private void EvalOperation(JProperty property, string arguments, State state, ref List<string> loopProperties, ref List<JToken> tokensToAdd)
        {
            object functionResult = ParseFunction(arguments, state);

            object val;
            if (property.Value.Type == JTokenType.String)
            {
                val = ParseFunction(property.Value.Value<string>(), state);
            }
            else
            {
                var propVal = property.Value;
                RecursiveEvaluate(ref propVal, state);
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

        private void BulkOperations(JEnumerable<JToken> arrayValues, State state, ref List<JToken> selectedTokens, ref Dictionary<string, JToken> tokensToReplace, ref List<JToken> tokensToDelete)
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
                        selectedTokens.Add(Copy(arguments, state));
                    }
                    else if (functionName == "replace")
                    {
                        if (tokensToReplace == null)
                            tokensToReplace = new Dictionary<string, JToken>();

                        var replaceResult = Replace(arguments, state);
                        tokensToReplace.Add(replaceResult.Key, replaceResult.Value);
                    }
                    else if (functionName == "delete")
                    {
                        if (tokensToDelete == null)
                            tokensToDelete = new List<JToken>();

                        tokensToDelete.Add(Delete(arguments, state));
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

        private IEnumerable<object> TransformArray(JEnumerable<JToken> children, State state)
        {
            var result = new List<object>();

            foreach (JToken arrEl in children)
            {
                object itemToAdd = arrEl.Value<JToken>();
                if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                {
                    itemToAdd = ParseFunction(arrEl.ToString(), state);
                }
                result.Add(itemToAdd);
            }

            return result;
        }

        #region Copy
        private JToken Copy(string arguments, State state)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            string path = argumentArr[0];
            if (!(ParseArgument(state, path) is string jsonPath))
            {
                throw new ArgumentException($"Invalid path for #copy: '{argumentArr[0]}' resolved to null!");
            }

            string alias = null;
            if (argumentArr.Length > 1)
            {
                alias = ParseArgument(state, argumentArr[1]) as string;
                if (!state.CurrentArrayToken.Any(a => a.Key.Key == alias))
                {
                    throw new ArgumentException($"Unknown loop alias: '{argumentArr[1]}'");
                }
            }
            JToken input = alias != null ? state.CurrentArrayToken.Single(a => a.Key.Key == alias).Value : state.CurrentArrayToken.Last().Value ?? Context.Input;
            JToken selectedToken = GetSelectableToken(input, Context).Select(jsonPath);
            return selectedToken;
        }

        #endregion

        #region Replace
        private KeyValuePair<string, JToken> Replace(string arguments, State state)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments, Context.EscapeChar);
            if (argumentArr.Length < 2)
            {
                throw new Exception("Function #replace needs at least two arguments - 1. path to be replaced, 2. token to replace with.");
            }
            if (!(ParseArgument(state, argumentArr[0]) is string key))
            {
                throw new ArgumentException($"Invalid path for #replace: '{argumentArr[0]}' resolved to null!");
            }
            object str = ParseArgument(state, argumentArr[1]);
            JToken newToken = GetToken(str);
            return new KeyValuePair<string, JToken>(key, newToken);
        }

        #endregion

        #region Delete
        private string Delete(string argument, State state)
        {
            if (!(ParseArgument(state, argument) is string result))
            {
                throw new ArgumentException($"Invalid path for #delete: '{argument}' resolved to null!");
            }
            return result;
        }
        #endregion

        #region ParseFunction

        private object ParseFunction(string functionString, State state)
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
                    output = GetConditionalOutput(arguments, state);
                }
                else
                {
                    int i = 0;
                    for (; i < (arguments?.Length ?? 0); i++)
                    {
                        output = ParseArgument(state, arguments[i]);
                        output = LookInTransformed(output, arguments[i], state);
                        listParameters.Add(output);
                    }
                    listParameters.Add(Context);

                    var convertParameters = true;
                    if (new[] { "concat", "xconcat", "currentproperty" }.Contains(functionName))
                    {
                        convertParameters = false;
                    }

                    output = GetFunctionOutput(functionName, listParameters, convertParameters, state);
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling function : " + functionString + " - " + ex.Message, ex);
            }
        }

        private object ParseApplyOver(IList<object> listParameters, State state)
        {
            object output;
            JToken tmpContext = Context.Input, contextInput = Context.Input;
            if (state.ParentArray.Any())
            {
                var alias = ParseLoopAlias(listParameters, 3, state.ParentArray.Last().Key.Key);
                contextInput = state.GetAliasToken(alias);
            }

            var input = JToken.Parse(Transform(listParameters.ElementAt(0).ToString(), contextInput.ToString()));
            Context.Input = input;
            
            IDictionary<LevelKey, JToken> tmpArray = new Dictionary<LevelKey, JToken>(state.CurrentArrayToken);
            IDictionary<LevelKey, JToken> tmpScope = new Dictionary<LevelKey, JToken>(state.CurrentScopeToken);

            state.CurrentArrayToken.Clear();
            state.CurrentArrayToken.Add(new LevelKey { Key = State.RootKey, Level = 0 }, input);

            state.CurrentScopeToken.Clear();
            state.CurrentScopeToken.Add(new LevelKey { Key = State.RootKey, Level = 0 }, input);

            if (listParameters.ElementAt(1).ToString().Trim().Trim('\'').StartsWith("{"))
            {
                var jobj = JObject.Parse(listParameters.ElementAt(1).ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jobj, input);
            }
            else if (listParameters.ElementAt(1).ToString().Trim().Trim('\'').StartsWith("["))
            {
                var jarr = JArray.Parse(listParameters.ElementAt(1).ToString().Trim().Trim('\''));
                output = new JsonTransformer(Context).Transform(jarr, input);
            }
            else
            {
                output = ParseFunction(listParameters.ElementAt(1).ToString().Trim().Trim('\''), state);
            }
            Context.Input = tmpContext;
            
            state.CurrentArrayToken.Clear();
            foreach (KeyValuePair<LevelKey, JToken> item in tmpArray)
            {
                state.CurrentArrayToken.Add(item);
            }
            
            state.CurrentScopeToken.Clear();
            foreach (KeyValuePair<LevelKey, JToken> item in tmpScope)
            {
                state.CurrentScopeToken.Add(item);
            }
            return output;
        }

        private string ParseLoopAlias(IList<object> listParameters, int index, string defaultValue)
        {
            string alias;
            if (listParameters != null && listParameters.Count > index)
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

        private object ParseArgument(State state, string argument)
        {
            object output = argument;
            var trimmedArgument = argument.Trim();
            if (trimmedArgument.StartsWith("#"))
            {
                return ParseFunction(trimmedArgument, state);
            }
            else if (trimmedArgument.StartsWith($"{Context.EscapeChar}#"))
            {
                output = ExpressionHelper.UnescapeSharp(argument, Context.EscapeChar);
            }
            return output;
        }

        private object GetConditionalOutput(string[] arguments, State state)
        {
            var condition = ParseArgument(state, arguments[0]);
            condition = LookInTransformed(condition, arguments[0], state);
            var value = ParseArgument(state, arguments[1]);
            value = LookInTransformed(value, arguments[1], state);
            
            var equal = ComparisonHelper.Equals(condition, value, Context.EvaluationMode);
            var index = equal ? 2 : 3;

            return ParseArgument(state, arguments[index]);
        }

        private object GetFunctionOutput(string functionName, IList<object> listParameters, bool convertParameters, State state)
        {
            object output = null;
            if (new[] { "currentvalue", "currentindex", "lastindex", "lastvalue" }.Contains(functionName))
            {
                var alias = ParseLoopAlias(listParameters, 1, state.ParentArray.Last().Key.Key);
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, new object[] { state.ParentArray.Single(p => p.Key.Key == alias).Value, state.CurrentArrayToken.Single(p => p.Key.Key == alias).Value }, convertParameters, Context);
            }
            else if (new[] { "currentvalueatpath", "lastvalueatpath" }.Contains(functionName))
            {
                var alias = ParseLoopAlias(listParameters, 2, state.ParentArray.Last().Key.Key);
                output = ReflectionHelper.Caller<T>(
                    null,
                    "JUST.Transformer`1",
                    functionName,
                    new[] { state.ParentArray.First(p => p.Key.Key == alias || p.Key.Key == state.ParentArray.Last().Key.Key) .Value, state.CurrentArrayToken.Single(p => p.Key.Key == alias).Value }.Concat(listParameters.ToArray()).ToArray(),
                    convertParameters,
                    Context);
            }
            else if (functionName == "currentproperty")
            {
                var alias = ParseLoopAlias(listParameters, 1, state.ParentArray.Last().Key.Key);
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName,
                    new object[] { state.ParentArray.Single(p => p.Key.Key == alias).Value, state.CurrentArrayToken.Single(p => p.Key.Key == alias).Value, Context },
                    convertParameters, Context);
            }
            else if (functionName == "customfunction")
                output = CallCustomFunction(listParameters.ToArray());
            else if (Context.IsRegisteredCustomFunction(functionName))
            {
                var methodInfo = Context.GetCustomMethod(functionName);
                output = ReflectionHelper.InvokeCustomMethod<T>(methodInfo, listParameters.ToArray(), convertParameters, Context.EvaluationMode);
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
                oParams[0] = listParameters.ToArray();
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, oParams, convertParameters, Context);
            }
            else if (functionName == "applyover")
            {
                output = ParseApplyOver(listParameters, state);
            }
            else
            {
                var input = ((JUSTContext)listParameters.Last()).Input;
                if (input != state.Transformer)
                {
                    if (functionName != "valueof")
                    {
                        // TODO Input must change to previous transformed output
                        // if (state.Transformer != null)
                        // {
                        //     ((JUSTContext)listParameters.Last()).Input = state.CurrentScopeToken.Last().Value;
                        // }
                        // else
                        // {
                            ((JUSTContext)listParameters.Last()).Input = state.CurrentArrayToken.Last().Value;
                        // }
                    }
                    else
                    {
                        if (listParameters.Count > 2)
                        {
                            ((JUSTContext)listParameters.Last()).Input = state.CurrentScopeToken.Single(p => p.Key.Key == listParameters[1].ToString()).Value;
                            listParameters.Remove(listParameters.ElementAt(listParameters.Count - 2));
                        }
                        else
                        {
                            ((JUSTContext)listParameters.Last()).Input = state.CurrentScopeToken.Last().Value;
                        }
                    }
                }
                
                output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, listParameters.ToArray(), convertParameters, Context);
                
                if (input != state.Transformer)
                {
                    ((JUSTContext)listParameters.Last()).Input = input;
                }
            }
            return output;
        }

        private object LookInTransformed(object output, string propVal, State state)
        {
            if (output == null && Context.IsLookInTransformed())
            {
                JToken tmpContext = Context.Input;
                Context.Input = state.Transformer.Root;
                output = ParseFunction(propVal, state);
                Context.Input = tmpContext;
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

        private static T GetSelectableToken(JToken token, JUSTContext context)
        {
            return context.Resolve<T>(token);
        }
    }
}
