using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JUST
{
    public static class JsonTransformer
    {
        public static readonly JUSTContext GlobalContext = new JUSTContext();

        static JsonTransformer()
        {
            if (JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                };
            }
        }

        public static string Transform(string transformerJson, string inputJson, JUSTContext localContext = null)
        {
            JToken result = null;
            JToken transformerToken = JsonConvert.DeserializeObject<JToken>(transformerJson);
            switch (transformerToken.Type)
            {
                case JTokenType.Object:
                    result = Transform(transformerToken as JObject, inputJson, localContext);
                    break;
                case JTokenType.Array:
                    result = Transform(transformerToken as JArray, inputJson, localContext);
                    break;
                default:
                    throw new NotSupportedException($"Transformer of type '{transformerToken.Type}' not supported!");
            }

            string output = JsonConvert.SerializeObject(result);

            return output;
        }

        public static JArray Transform(JArray transformerArray, string input, JUSTContext localContext = null)
        {
            var result = new JArray();
            foreach (var transformer in transformerArray)
            {
                if (transformer.Type != JTokenType.Object)
                {
                    throw new NotSupportedException($"Transformer of type '{transformer.Type}' not supported!");
                }
                Transform(transformer as JObject, input, localContext);
                result.Add(transformer);
            }
            return result;
        }

        public static JObject Transform(JObject transformer, JObject input, JUSTContext localContext = null)
        {
            string inputJson = JsonConvert.SerializeObject(input);
            return Transform(transformer, inputJson, localContext);
        }

        public static JObject Transform(JObject transformer, string input, JUSTContext localContext = null)
        {
            (localContext ?? GlobalContext).Input = JsonConvert.DeserializeObject<JToken>(input);
            RecursiveEvaluate(transformer, input, null, null, localContext);
            return transformer;
        }
        #region RecursiveEvaluate


        private static void RecursiveEvaluate(JToken parentToken, string inputJson, JArray parentArray, JToken currentArrayToken, JUSTContext localContext)
        {
            if (parentToken == null)
                return;

            JEnumerable<JToken> tokens = parentToken.Children();

            List<JToken> selectedTokens = null;
            Dictionary<string, JToken> tokensToReplace = null;
            List<JToken> tokensToDelete = null;

            List<string> loopProperties = null;
            JArray arrayToForm = null;
            List<JToken> tokenToForm = null;
            List<JToken> tokensToAdd = null;

            bool isLoop = false;

            foreach (JToken childToken in tokens)
            {
                if (childToken.Type == JTokenType.Array && (parentToken as JProperty).Name.Trim() != "#")
                {
                    JArray arrayToken = childToken as JArray;

                    List<object> itemsToAdd = new List<object>();

                    foreach (JToken arrEl in childToken.Children())
                    {
                        object itemToAdd = arrEl.Value<JToken>();

                        if (arrEl.Type == JTokenType.String && arrEl.ToString().Trim().StartsWith("#"))
                        {
                            object value = ParseFunction(arrEl.ToString(), inputJson, parentArray, currentArrayToken, localContext);
                            itemToAdd = value;
                        }

                        itemsToAdd.Add(itemToAdd);
                    }

                    arrayToken.RemoveAll();

                    foreach (object itemToAdd in itemsToAdd)
                    {
                        arrayToken.Add(itemToAdd);
                    }
                }

                if (childToken.Type == JTokenType.Property)
                {
                    JProperty property = childToken as JProperty;

                    if (property.Name != null && property.Name == "#" && property.Value.Type == JTokenType.Array)
                    {
                        JArray values = property.Value as JArray;

                        JEnumerable<JToken> arrayValues = values.Children();

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

                                    selectedTokens.Add(Copy(arguments, GetInputToken(localContext)));
                                }
                                else if (functionName == "replace")
                                {
                                    if (tokensToReplace == null)
                                        tokensToReplace = new Dictionary<string, JToken>();

                                    tokensToReplace.Add(GetTokenStringToReplace(arguments), Replace(arguments, inputJson, localContext));
                                }
                                else if (functionName == "delete")
                                {
                                    if (tokensToDelete == null)
                                        tokensToDelete = new List<JToken>();

                                    tokensToDelete.Add(arguments);
                                }
                            }
                        }
                    }

                    if (property.Name != null && property.Value.ToString().Trim().StartsWith("#")
                        && !property.Name.Contains("#eval")  && !property.Name.Contains("#ifgroup")
                        && !property.Name.Contains("#loop"))
                    {
                        object newValue = ParseFunction(property.Value.ToString(), inputJson, parentArray, currentArrayToken, localContext);

                        //TODO Check return object for JToken or string with double quotes!
                        if (newValue != null && newValue.ToString().Contains("\""))
                        {
                            try
                            {
                                JToken newToken = JToken.FromObject(newValue);
                                property.Value = newToken;
                            }
                            catch
                            {
                                property.Value = new JValue(newValue);
                            }
                        }
                        else
                        {
                            var arr = newValue as IEnumerable<object>;
                            if (arr != null)
                            {
                                property.Value = new JArray(arr);
                            }
                            else
                            {
                                property.Value = new JValue(newValue);
                            }
                        }
                    }

                    /* For looping*/
                    isLoop = false;

                    if (property.Name != null && property.Name.Contains("#eval"))
                    {
                        ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string functionString);
                        object functionResult = ParseFunction(functionString, inputJson, null, null, localContext);

                        JProperty clonedProperty = new JProperty(functionResult.ToString(), property.Value);

                        if (loopProperties == null)
                            loopProperties = new List<string>();

                        loopProperties.Add(property.Name);

                        if (tokensToAdd == null)
                        {
                            tokensToAdd = new List<JToken>
                            {
                                clonedProperty
                            };
                        }
                    }

                    if (property.Name != null && property.Name.Contains("#ifgroup"))
                    {
                        ExpressionHelper.TryParseFunctionNameAndArguments(property.Name, out string functionName, out string functionString);
                        object functionResult = ParseFunction(functionString, inputJson, null, null, localContext);
                        bool result = false;

                        try
                        {
                            result = Convert.ToBoolean(functionResult);
                        }
                        catch
                        {
                            if (IsStrictMode(localContext)) { throw; }
                            result =  false;
                        }

                        if (result == true)
                        {
                            if (loopProperties == null)
                                loopProperties = new List<string>();

                            loopProperties.Add(property.Name);

                            RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken, localContext);

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
                                loopProperties = new List<string>();

                            loopProperties.Add(property.Name);
                        }

                        isLoop = true;
                    }

                    if (property.Name != null && property.Name.Contains("#loop"))
                    {
                        string strArrayToken = property.Name.Substring(6, property.Name.Length - 7);

                        var jsonToLoad = inputJson;
                        if (currentArrayToken != null && property.Name.Contains("#loopwithincontext"))
                        {
                            strArrayToken = property.Name.Substring(19, property.Name.Length - 20);
                            jsonToLoad = JsonConvert.SerializeObject(currentArrayToken);
                        }
                        
                        JToken token = JsonConvert.DeserializeObject<JObject>(jsonToLoad);
                        JToken arrayToken = null;
                        if (strArrayToken.Contains("#"))
                        {
                            int sIndex = strArrayToken.IndexOf("#");
                            string sub1 = strArrayToken.Substring(0, sIndex);

                            int indexOfENdFubction = GetIndexOfFunctionEnd(strArrayToken);

                            if (indexOfENdFubction > sIndex && sIndex > 0)
                            {
                                string sub2 = strArrayToken.Substring(indexOfENdFubction + 1, strArrayToken.Length - indexOfENdFubction - 1);

                                string functionResult = ParseFunction(strArrayToken.Substring(sIndex, indexOfENdFubction - sIndex + 1), inputJson, parentArray, currentArrayToken, localContext).ToString();

                                strArrayToken = sub1 + functionResult + sub2;
                            }
                        }
                        try
                        {
                            arrayToken = token.SelectToken(strArrayToken);

                            if (arrayToken is JObject)
                            {
                                arrayToken = new JArray(arrayToken);
                            }
                        }
                        catch
                        {
                            var multipleTokens = token.SelectTokens(strArrayToken);

                            arrayToken = new JArray(multipleTokens);
                        }

                        if (arrayToken == null)
                        {
                            arrayToForm = new JArray();
                        }
                        else
                        {
                            JArray array = (JArray)arrayToken;

                            IEnumerator<JToken> elements = array.GetEnumerator();

                            while (elements.MoveNext())
                            {
                                if (arrayToForm == null)
                                    arrayToForm = new JArray();

                                JToken clonedToken = childToken.DeepClone();

                                RecursiveEvaluate(clonedToken, inputJson, array, elements.Current, localContext);

                                foreach (JToken replacedProperty in clonedToken.Children())
                                {
                                    arrayToForm.Add(replacedProperty);
                                }
                            }
                        }
                        if (loopProperties == null)
                            loopProperties = new List<string>();

                        loopProperties.Add(property.Name);
                        isLoop = true;
                    }
                    /*End looping */
                }

                if (childToken.Type == JTokenType.String && childToken.Value<string>().Trim().StartsWith("#") 
                    && parentArray != null && currentArrayToken != null)
                {
                    object newValue = ParseFunction(childToken.Value<string>(), inputJson, parentArray, currentArrayToken, localContext);

                    //TODO Check return object for JToken or string with double quotes!
                    if (newValue != null && newValue.ToString().Contains("\""))
                    {
                        try
                        {
                            JToken newToken = JToken.FromObject(newValue);
                            childToken.Replace(new JValue(newValue));
                        }
                        catch
                        {
                            if (IsStrictMode(localContext)) { throw; }
                            if (IsFallbackToNull(localContext)) { childToken.Replace(JValue.CreateNull()); };
                        }
                    }
                    else
                        childToken.Replace(new JValue(newValue));
                }

                if (!isLoop)
                    RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken, localContext);
            }

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

                    if (selectedToken != null && selectedToken is JObject)
                    {
                        JObject selectedObject = selectedToken as JObject;
                        selectedObject.RemoveAll();

                        JEnumerable<JToken> copyChildren = tokenToReplace.Value.Children();

                        foreach (JToken copyChild in copyChildren)
                        {
                            JProperty property = copyChild as JProperty;
                            selectedObject.Add(property.Name, property.Value);
                        }
                    }
                    if (selectedToken != null && selectedToken is JValue)
                    {
                        JValue selectedObject = selectedToken as JValue;
                        selectedObject.Value = tokenToReplace.Value.ToString();
                    }
                }
            }

            if (tokensToDelete != null)
            {
                foreach (string selectedToken in tokensToDelete)
                {
                    JToken tokenToRemove = parentToken.SelectToken(selectedToken);

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
            if (parentToken is JObject)
            {
                (parentToken as JObject).Remove("#");
            }

            if (loopProperties != null)
            {
                foreach (string propertyToDelete in loopProperties)
                    (parentToken as JObject).Remove(propertyToDelete);
            }
            if (arrayToForm != null)
            {
                parentToken.Replace(arrayToForm);
            }

        }
        #endregion

        #region Copy
        private static JToken Copy(string jsonPath, JToken token)
        {
            JToken selectedToken = token.SelectToken(jsonPath);
            return selectedToken;
        }

        #endregion

        #region Replace
        private static JToken Replace(string argumentString, string inputJson, JUSTContext localContext)
        {
            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");

            JToken newToken = null;
            object str = ParseFunction(arguments[1], inputJson, null, null, localContext);

            //TODO Check return object for JToken or string with double quotes!
            if (str != null && str.ToString().Contains("\""))
            {
                newToken = JToken.FromObject(str);

            }
            else
                newToken = str.ToString();

            return newToken;
        }

        private static string GetTokenStringToReplace(string argumentString)
        {
            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");
            return arguments[0];

        }

        #endregion

        #region ParseFunction

        private static object ParseFunction(string functionString, string inputJson, JArray array, JToken currentArrayElement, JUSTContext localContext)
        {
            try
            {
                object output = null;

                string functionName, argumentString;
                if (!ExpressionHelper.TryParseFunctionNameAndArguments(functionString, out functionName, out argumentString))
                {
                    return functionName;
                }

                string[] arguments = ExpressionHelper.GetArguments(argumentString);
                var listParameters = new List<object>();

                if (functionName == "ifcondition")
                {
                    var condition = ParseArgument(inputJson, array, currentArrayElement, arguments[0], localContext);
                    var value = ParseArgument(inputJson, array, currentArrayElement, arguments[1], localContext);
                    var index = condition.ToString().ToLower() == value.ToString().ToLower() ? 2 : 3;
                    output = ParseArgument(inputJson, array, currentArrayElement, arguments[index], localContext);
                }
                else
                {
                    int i = 0;
                    for (; i < (arguments?.Length ?? 0); i++)
                    {
                        listParameters.Add(ParseArgument(inputJson, array, currentArrayElement, arguments[i], localContext));
                    }

                    listParameters.Add(localContext ?? GlobalContext);
                    var parameters = listParameters.ToArray();

                    if (functionName == "currentvalue" || functionName == "currentindex" || functionName == "lastindex"
                        || functionName == "lastvalue")
                        output = ReflectionHelper.caller(null, "JUST.Transformer", functionName, new object[] { array, currentArrayElement }, true, localContext ?? GlobalContext);
                    else if (functionName == "currentvalueatpath" || functionName == "lastvalueatpath")
                        output = ReflectionHelper.caller(null, "JUST.Transformer", functionName, new object[] { array, currentArrayElement, arguments[0] }, true, localContext ?? GlobalContext);
                    else if (functionName == "customfunction")
                        output = CallCustomFunction(parameters, localContext);
                    else if (localContext?.IsRegisteredCustomFunction(functionName) ?? false)
                    {
                        var methodInfo = localContext.GetCustomMethod(functionName);
                        output = ReflectionHelper.InvokeCustomMethod(methodInfo, parameters, true, localContext ?? GlobalContext);
                    }
                    else if (GlobalContext.IsRegisteredCustomFunction(functionName))
                    {
                        var methodInfo = GlobalContext.GetCustomMethod(functionName);
                        output = ReflectionHelper.InvokeCustomMethod(methodInfo, parameters, true, localContext ?? GlobalContext);
                    }
                    else if (Regex.IsMatch(functionName, ReflectionHelper.EXTERNAL_ASSEMBLY_REGEX))
                    {
                        output = ReflectionHelper.CallExternalAssembly(functionName, parameters, localContext ?? GlobalContext);
                    }
                    else if (functionName == "xconcat" || functionName == "xadd"
                        || functionName == "mathequals" || functionName == "mathgreaterthan" || functionName == "mathlessthan"
                        || functionName == "mathgreaterthanorequalto"
                        || functionName == "mathlessthanorequalto" || functionName == "stringcontains" ||
                        functionName == "stringequals")
                    {
                        object[] oParams = new object[1];
                        oParams[0] = parameters;
                        output = ReflectionHelper.caller(null, "JUST.Transformer", functionName, oParams, true, localContext ?? GlobalContext);
                    }
                    else
                    {
                        var input = ((JUSTContext)parameters.Last()).Input;
                        if (currentArrayElement != null && functionName != "valueof")
                        {
                            ((JUSTContext)parameters.Last()).Input = JsonConvert.SerializeObject(currentArrayElement);
                        }
                        output = ReflectionHelper.caller(null, "JUST.Transformer", functionName, parameters, true, localContext ?? GlobalContext);
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

        private static object ParseArgument(string inputJson, JArray array, JToken currentArrayElement, string argument, JUSTContext localContext)
        {
            string trimmedArgument = argument;

            if (argument.Contains("#"))
                trimmedArgument = argument.Trim();

            if (trimmedArgument.StartsWith("#"))
            {
                return ParseFunction(trimmedArgument, inputJson, array, currentArrayElement, localContext);
            }
            else
                return trimmedArgument;
        }

        private static object CallCustomFunction(object[] parameters, JUSTContext localContext)
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

            return ReflectionHelper.caller(null, className, functionName, customParameters, true, localContext ?? GlobalContext);

        }
        #endregion

        #region Split
        public static IEnumerable<string> SplitJson(string input, string arrayPath)
        {
            JObject inputJObject = JsonConvert.DeserializeObject<JObject>(input);

            List<JObject> jObjects = SplitJson(inputJObject, arrayPath).ToList<JObject>();

            List<string> output = null;

            foreach (JObject jObject in jObjects)
            {
                if (output == null)
                    output = new List<string>();

                output.Add(JsonConvert.SerializeObject(jObject));
            }

            return output;
        }

        public static IEnumerable<JObject> SplitJson(JObject input, string arrayPath)
        {
            List<JObject> jsonObjects = null;

            JToken tokenArr = input.SelectToken(arrayPath);

            string pathToReplace = tokenArr.Path;

            if (tokenArr != null && tokenArr is JArray)
            {
                JArray array = tokenArr as JArray;

                foreach (JToken tokenInd in array)
                {

                    string path = tokenInd.Path;

                    JToken clonedToken = input.DeepClone();

                    JToken foundToken = clonedToken.SelectToken("$." + path);
                    JToken tokenToReplcae = clonedToken.SelectToken("$." + pathToReplace);

                    tokenToReplcae.Replace(foundToken);

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

        private static JToken GetInputToken(JUSTContext localContext)
        {
            return localContext?.Input ?? GlobalContext.Input;
        }

        private static EvaluationMode GetEvaluationMode(JUSTContext localContext)
        {
            return localContext?.EvaluationMode ?? GlobalContext.EvaluationMode;
        }

        private static bool IsStrictMode(JUSTContext localContext)
        {
            return GetEvaluationMode(localContext) == EvaluationMode.Strict;
        }

        private static bool IsFallbackToNull(JUSTContext localContext)
        {
            return GetEvaluationMode(localContext) == EvaluationMode.FallbackToNull;
        }
    }
}
