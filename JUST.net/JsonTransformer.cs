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

    public class JsonTransformer<T> where T: ISelectableToken
    {
        private readonly JUSTContext Context;

        public JsonTransformer(JUSTContext context = null)
        {
            Context = context ?? new JUSTContext();

            if (JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                };
            }
        }

        public string Transform(string transformerJson, string inputJson)
        {
            return Transform(transformerJson, JsonConvert.DeserializeObject<JToken>(inputJson));
        }

        public string Transform(string transformerJson, JToken input)
        {
            JToken result = null;
            JToken transformerToken = JsonConvert.DeserializeObject<JToken>(transformerJson);
            switch (transformerToken.Type)
            {
                case JTokenType.Object:
                    result = Transform(transformerToken as JObject, input);
                    break;
                case JTokenType.Array:
                    result = Transform(transformerToken as JArray, input);
                    break;
                default:
                    throw new NotSupportedException($"Transformer of type '{transformerToken.Type}' not supported!");
            }

            string output = JsonConvert.SerializeObject(result);

            return output;
        }

        public JArray Transform(JArray transformerArray, string input)
        {
            return Transform(transformerArray, JsonConvert.DeserializeObject<JToken>(input));
        }

        public JArray Transform(JArray transformerArray, JToken input)
        {
            var result = new JArray();
            for (int i = 0; i < transformerArray.Count; i++) {
                var transformer = transformerArray[i];
                if (transformer.Type != JTokenType.Object)
                {
                    throw new NotSupportedException($"Transformer of type '{transformer.Type}' not supported!");
                }
                Transform(transformer as JObject, input);
                result.Add(transformer);
            }
            return result;
        }

        public JObject Transform(JObject transformer, string input)
        {
            return Transform(transformer, JsonConvert.DeserializeObject<JToken>(input));
        }

        public JObject Transform(JObject transformer, JToken input)
        {
            Context.Input = input;
            RecursiveEvaluate(transformer, null, null);
            return transformer;
        }

        #region RecursiveEvaluate


        private void RecursiveEvaluate(JToken parentToken, JArray parentArray, JToken currentArrayToken)
        {
            if (parentToken == null)
                return;

            JEnumerable<JToken> tokens = parentToken.Children();

            List<JToken> selectedTokens = null;
            Dictionary<string, JToken> tokensToReplace = null;
            List<JToken> tokensToDelete = null;

            List<string> loopProperties = null;
            JArray arrayToForm = null;
            JObject dictToForm = null;
            List<JToken> tokenToForm = null;
            List<JToken> tokensToAdd = null;

            bool isLoop = false;
            bool isBulk = false;

            foreach (JToken childToken in tokens)
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
                    else if (property.Name.TrimStart().StartsWith("#eval"))
                    {
                        EvalOperation(property, parentArray, currentArrayToken, ref loopProperties, ref tokensToAdd);
                    }
                    else if (property.Name.TrimStart().Contains("#ifgroup"))
                    {
                        ConditionalGroupOperation(property, parentArray, currentArrayToken, ref loopProperties, ref tokenToForm, childToken);

                        isLoop = true;
                    }
                    else if (property.Name.TrimStart().Contains("#loop"))
                    {
                        LoopOperation(property, parentArray, currentArrayToken, ref loopProperties, ref arrayToForm, ref dictToForm, childToken);
                        isLoop = true;
                    }
                    else if (property.Value.ToString().Trim().StartsWith("#"))
                    {
                        property.Value = GetToken(ParseFunction(property.Value.ToString(), parentArray, currentArrayToken));
                    }

                    if (property.Name != null && property.Value.ToString().StartsWith($"{ExpressionHelper.EscapeChar}#"))
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
                    RecursiveEvaluate(childToken, parentArray, currentArrayToken);
                }
            }

            parentToken = PostOperationsBuildUp(parentToken, selectedTokens, tokensToReplace, tokensToDelete, loopProperties, arrayToForm, dictToForm, tokenToForm, tokensToAdd);
        }

        private JToken PostOperationsBuildUp(JToken parentToken, List<JToken> selectedTokens, Dictionary<string, JToken> tokensToReplace, List<JToken> tokensToDelete, List<string> loopProperties, JArray arrayToForm, JObject dictToForm, List<JToken> tokenToForm, List<JToken> tokensToAdd)
        {
            if (selectedTokens != null)
            {
                foreach (JToken selectedToken in selectedTokens)
                {
                    if (selectedToken != null)
                    {
                        JEnumerable<JToken> copyChildren = selectedToken.Children();

                        foreach (JToken copyChild in copyChildren)
                        {
                            JProperty property = copyChild as JProperty;

                            (parentToken as JObject).Add(property.Name, property.Value);
                        }
                    }
                }
            }

            if (tokensToReplace != null)
            {
                foreach (KeyValuePair<string, JToken> tokenToReplace in tokensToReplace)
                {
                    JToken selectedToken = (parentToken as JObject).SelectToken(tokenToReplace.Key);
                    selectedToken.Replace(tokenToReplace.Value);
                }
            }

            if (tokensToDelete != null)
            {
                foreach (string selectedToken in tokensToDelete)
                {
                    JToken tokenToRemove = GetSelectableToken(parentToken, Context).Select(selectedToken);

                    if (tokenToRemove != null)
                        tokenToRemove.Ancestors().First().Remove();
                }
            }
            if (tokensToAdd != null)
            {
                foreach (JToken token in tokensToAdd)
                {
                    (parentToken as JObject).Add((token as JProperty).Name, (token as JProperty).Value);
                }
            }

            if (tokenToForm != null)
            {
                foreach (JToken token in tokenToForm)
                {
                    foreach (JProperty childToken in token.Children())
                        (parentToken as JObject).Add(childToken.Name, childToken.Value);
                }
            }
            if (parentToken is JObject jObject)
            {
                jObject.Remove("#");
            }

            if (loopProperties != null)
            {
                foreach (string propertyToDelete in loopProperties)
                {
                    if (dictToForm == null && arrayToForm == null && parentToken.Count() <= 1)
                    {
                        parentToken.Replace(JValue.CreateNull());
                    }
                    else
                    {
                        (parentToken as JObject).Remove(propertyToDelete);
                    }
                }
            }

            if (dictToForm != null)
            {
                parentToken.Replace(dictToForm);
            }
            else if (arrayToForm != null)
            {
                if (parentToken.Parent != null && parentToken.Parent is JArray arr)
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
                else if (parentToken.Parent != null)
                {
                    parentToken.Replace(arrayToForm);
                }
            }

            return parentToken;
        }

        private void LoopOperation(JProperty property, JArray parentArray, JToken currentArrayToken, ref List<string> loopProperties, ref JArray arrayToForm, ref JObject dictToForm, JToken childToken)
        {
            ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments);
            var token = currentArrayToken != null && functionName == "loopwithincontext" ? currentArrayToken : Context.Input;

            var strArrayToken = ParseArgument(parentArray, currentArrayToken, arguments) as string;

            bool isDictionary = false;
            JToken arrayToken;
            var selectable = GetSelectableToken(token, Context);
            arrayToken = selectable.Select(strArrayToken);
            
            //workaround: result should be an array if path ends up with array filter
            if (typeof(T) == typeof(JsonPathSelectable) && Regex.IsMatch(strArrayToken ?? string.Empty, "\\[.+\\]$") && arrayToken.Type != JTokenType.Array)
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

            JArray array = arrayToken as JArray;

            if (array != null)
            {
                using (IEnumerator<JToken> elements = array.GetEnumerator())
                {
                    arrayToForm = new JArray();
                    if (!isDictionary)
                    {
                        while (elements.MoveNext())
                        {
                            JToken clonedToken = childToken.DeepClone();
                            RecursiveEvaluate(clonedToken, array, elements.Current);

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
                            RecursiveEvaluate(clonedToken, array, elements.Current);
                            foreach (JToken replacedProperty in clonedToken.Children().Select(t => t.First))
                            {
                                dictToForm.Add(replacedProperty);
                            }
                        }
                    }
                }
            }

            if (loopProperties == null)
                loopProperties = new List<string>();

            loopProperties.Add(property.Name);
        }

        private void ConditionalGroupOperation(JProperty property, JArray parentArray, JToken currentArrayToken, ref List<string> loopProperties, ref List<JToken> tokenToForm, JToken childToken)
        {
            ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments);
            object functionResult = ParseFunction(arguments, parentArray, currentArrayToken);
            bool result = false;

            try
            {
                result = (bool)ReflectionHelper.GetTypedValue(typeof(bool), functionResult, Context.EvaluationMode);
            }
            catch
            {
                if (IsStrictMode(Context)) { throw; }
                result = false;
            }

            if (result == true)
            {
                if (loopProperties == null)
                    loopProperties = new List<string>();

                loopProperties.Add(property.Name);

                RecursiveEvaluate(childToken, parentArray, currentArrayToken);

                if (tokenToForm == null)
                {
                    tokenToForm = new List<JToken>();
                }

                foreach (JToken grandChildToken in childToken.Children())
                    tokenToForm.Add(grandChildToken.DeepClone());
            }
            else
            {
                if (loopProperties == null)
                {
                    loopProperties = new List<string>();
                }

                loopProperties.Add(property.Name);
            }
        }

        private void EvalOperation(JProperty property, JArray parentArray, JToken currentArrayToken, ref List<string> loopProperties, ref List<JToken> tokensToAdd)
        {
            ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string arguments);
            object functionResult = ParseFunction(arguments, parentArray, currentArrayToken);

            JProperty clonedProperty = new JProperty(functionResult.ToString(),
                property.Value.Type != JTokenType.Null ?
                    ReflectionHelper.GetTypedValue(
                        property.Value.Type,
                        ParseFunction(
                            property.Value.Value<string>(),
                            parentArray,
                            currentArrayToken),
                        Context.EvaluationMode) :
                    null);

            if (loopProperties == null)
                loopProperties = new List<string>();

            loopProperties.Add(property.Name);

            if (tokensToAdd == null)
            {
                tokensToAdd = new List<JToken>();
            }
            tokensToAdd.Add(clonedProperty);
        }

        private void BulkOperations(JEnumerable<JToken> arrayValues, JArray parentArray, JToken currentArrayToken, ref List<JToken> selectedTokens, ref Dictionary<string, JToken> tokensToReplace, ref List<JToken> tokensToDelete)
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
                    arrayToken.Add(JToken.FromObject(itemToAdd));
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
                        if (IsStrictMode(Context))
                        {
                            throw;
                        }

                        if (IsFallbackToDefault(Context))
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

        private IEnumerable<object> TransformArray(JEnumerable<JToken> children, JArray parentArray, JToken currentArrayToken)
        {
            var result = new List<object>();

            foreach (JToken arrEl in children)
            {
                object itemToAdd = arrEl.Value<JToken>();

                if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                {
                    object value = ParseFunction(arrEl.ToString(), parentArray, currentArrayToken);
                    itemToAdd = value;
                }

                result.Add(itemToAdd);
            }

            return result;
        }

        #region Copy
        private JToken Copy(string argument, JArray parentArray, JToken currentArrayElement)
        {
            if (!(ParseArgument(parentArray, currentArrayElement, argument) is string jsonPath))
            {
                throw new ArgumentException($"Invalid path for #copy: '{argument}' resolved to null");
            }
            JToken selectedToken = GetSelectableToken(currentArrayElement ?? Context.Input,Context).Select(jsonPath);
            return selectedToken;
        }

        #endregion

        #region Replace
        private KeyValuePair<string, JToken> Replace(string arguments, JArray parentArray, JToken currentArrayElement)
        {
            string[] argumentArr = ExpressionHelper.SplitArguments(arguments);
            if (argumentArr.Length < 2)
            {
                throw new Exception("Function #replace needs two arguments - 1. jsonPath to be replaced, 2. token to replace with.");
            }
            if (!(ParseArgument(parentArray, currentArrayElement, argumentArr[0]) is string key))
            {
                throw new ArgumentException("Invalid jsonPath for #replace!");
            }
            object str = ParseArgument(parentArray, currentArrayElement, argumentArr[1]);

            JToken newToken = GetToken(str); 
            return new KeyValuePair<string, JToken>(key, newToken);
        }

        #endregion

        #region Delete
        private string Delete(string argument, JArray parentArray, JToken currentArrayElement)
        {
            if (!(ParseArgument(parentArray, currentArrayElement, argument) is string result))
            {
                throw new ArgumentException("Invalid jsonPath for #delete!");
            }
            return result;
        }
        #endregion

        #region ParseFunction

        private object ParseFunction(string functionString, JArray array, JToken currentArrayElement)
        {
            try
            {
                object output = null;

                string functionName, argumentString;
                if (!ExpressionHelper.TryParseFunctionNameAndArguments(functionString, out functionName, out argumentString))
                {
                    return functionName;
                }

                string[] arguments = ExpressionHelper.SplitArguments(argumentString);
                var listParameters = new List<object>();

                if (functionName == "ifcondition")
                {
                    var condition = ParseArgument(array, currentArrayElement, arguments[0]);
                    var value = ParseArgument(array, currentArrayElement, arguments[1]);
                    var index = condition.ToString().ToLower() == value.ToString().ToLower() ? 2 : 3;
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
                    if(new[] { "concat", "xconcat", "currentproperty"}.Contains(functionName))
                    {
                        convertParameters = false;
                    }

                    if (new[] { "currentvalue", "currentindex", "lastindex", "lastvalue" }.Contains(functionName))
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, new object[] { array, currentArrayElement }, convertParameters, Context);
                    else if (new[] { "currentvalueatpath", "lastvalueatpath" }.Contains(functionName))
                        output = ReflectionHelper.Caller<T>(
                            null, 
                            "JUST.Transformer`1", 
                            functionName, 
                            new [] { array, currentArrayElement }.Concat(parameters).ToArray(), 
                            convertParameters, 
                            Context);
                    else if (functionName == "currentproperty")
                        output = ReflectionHelper.Caller<T>(null, "JUST.Transformer`1", functionName, 
                            new object[] { array, currentArrayElement, Context }, 
                            convertParameters, Context);
                    else if (functionName == "customfunction")
                        output = CallCustomFunction(parameters);
                    else if (Context?.IsRegisteredCustomFunction(functionName) ?? false)
                    {
                        var methodInfo = Context.GetCustomMethod(functionName);
                        output = ReflectionHelper.InvokeCustomMethod<T>(methodInfo, parameters, convertParameters, Context);
                    }
                    else if (Context.IsRegisteredCustomFunction(functionName))
                    {
                        var methodInfo = Context.GetCustomMethod(functionName);
                        output = ReflectionHelper.InvokeCustomMethod<T>(methodInfo, parameters, convertParameters, Context);
                    }
                    else if (Regex.IsMatch(functionName, ReflectionHelper.EXTERNAL_ASSEMBLY_REGEX))
                    {
                        output = ReflectionHelper.CallExternalAssembly<T>(functionName, parameters, Context);
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
                        var contextInput = Context.Input;
                        var input = JToken.Parse(Transform(parameters[0].ToString(), contextInput.ToString()));
                        Context.Input = input;
                        output = ParseFunction(parameters[1].ToString().Trim().Trim('\''), array, currentArrayElement);
                        Context.Input = contextInput;
                    }
                    else
                    {
                        var input = ((JUSTContext)parameters.Last()).Input;
                        if (currentArrayElement != null && functionName != "valueof")
                        {
                            ((JUSTContext)parameters.Last()).Input = currentArrayElement;
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

        private object ParseArgument(JArray array, JToken currentArrayElement, string argument)
        {
            var trimmedArgument = argument.Trim();
            if (trimmedArgument.StartsWith("#"))
            {
                return ParseFunction(trimmedArgument, array, currentArrayElement);
            }
            if (trimmedArgument.StartsWith($"{ExpressionHelper.EscapeChar}#"))
            {
                return ExpressionHelper.UnescapeSharp(argument);
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
            JObject inputJObject = JsonConvert.DeserializeObject<JObject>(input);

            List<JObject> jObjects = SplitJson(inputJObject, arrayPath, context).ToList();

            List<string> output = null;

            foreach (JObject jObject in jObjects)
            {
                if (output == null)
                    output = new List<string>();

                output.Add(JsonConvert.SerializeObject(jObject));
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

        private static bool IsStrictMode(JUSTContext context)
        {
            return context.EvaluationMode == EvaluationMode.Strict;
        }

        private static bool IsFallbackToDefault(JUSTContext context)
        {
            return context.EvaluationMode == EvaluationMode.FallbackToDefault;
        }

        private static T GetSelectableToken(JToken token, JUSTContext context)
        {
            return context.Resolve<T>(token);
        }
    }
}
