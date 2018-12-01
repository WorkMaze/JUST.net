using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace JUST
{
    public class JsonTransformer
    {
        public static string Transform(string transformerJson, string inputJson)
        {


            JToken transformerToken = JObject.Parse(transformerJson);

            RecursiveEvaluate(transformerToken, inputJson, null, null);

            string output = JsonConvert.SerializeObject(transformerToken);

            return output;
        }


        public static JObject Transform(JObject transformer, JObject input)
        {
            JObject transformerToken = transformer;
            string inputJson = JsonConvert.SerializeObject(input);
            RecursiveEvaluate(transformerToken, inputJson, null, null);
            return transformerToken;
        }
        #region RecursiveEvaluate


        private static void RecursiveEvaluate(JToken parentToken, string inputJson, JArray parentArray, JToken currentArrayToken)
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
                            object value = ParseFunction(arrEl.ToString(), inputJson, parentArray, currentArrayToken);
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
                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#copy"))
                            {
                                if (selectedTokens == null)
                                    selectedTokens = new List<JToken>();

                                selectedTokens.Add(Copy(arrayValue.Value<string>(), inputJson));


                            }

                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#replace"))
                            {
                                if (tokensToReplace == null)
                                    tokensToReplace = new Dictionary<string, JToken>();
                                string value = arrayValue.Value<string>();

                                tokensToReplace.Add(GetTokenStringToReplace(value), Replace(value, inputJson));


                            }

                            if (arrayValue.Type == JTokenType.String && arrayValue.Value<string>().Trim().StartsWith("#delete"))
                            {
                                if (tokensToDelete == null)
                                    tokensToDelete = new List<JToken>();


                                tokensToDelete.Add(Delete(arrayValue.Value<string>()));


                            }
                        }

                    }

                    if (property.Name != null && property.Value.ToString().Trim().StartsWith("#")
                        && !property.Name.Contains("#eval")  && !property.Name.Contains("#ifgroup")
                        && !property.Name.Contains("#loop"))
                    {
                        object newValue = ParseFunction(property.Value.ToString(), inputJson, parentArray, currentArrayToken);

                        if (newValue != null && newValue.ToString().Contains("\""))
                        {
                            try
                            {
                                JToken newToken = JToken.Parse(newValue.ToString());
                                property.Value = newToken;
                            }
                            catch
                            {
                                property.Value = new JValue(newValue);
                            }
                        }
                        else
                            property.Value = new JValue(newValue);
                    }

                    /* For looping*/
                    isLoop = false;

                    if (property.Name != null && property.Name.Contains("#eval"))
                    {
                        int startIndex = property.Name.IndexOf("(");
                        int endIndex = property.Name.LastIndexOf(")");

                        string functionString = property.Name.Substring(startIndex + 1, endIndex - startIndex - 1);

                        object functionResult = ParseFunction(functionString, inputJson, null, null);

                        JProperty clonedProperty = new JProperty(functionResult.ToString(), property.Value);

                        if (loopProperties == null)
                            loopProperties = new List<string>();

                        loopProperties.Add(property.Name);

                        if (tokensToAdd == null)
                        {
                            tokensToAdd = new List<JToken>();

                            tokensToAdd.Add(clonedProperty);
                        }



                    }

                    if (property.Name != null && property.Name.Contains("#ifgroup"))
                    {
                        int startIndex = property.Name.IndexOf("(");
                        int endIndex = property.Name.LastIndexOf(")");

                        string functionString = property.Name.Substring(startIndex + 1, endIndex - startIndex - 1);

                        object functionResult = ParseFunction(functionString, inputJson, null, null);
                        bool result = false;

                        try
                        {
                            result = Convert.ToBoolean(functionResult);
                        }
                        catch
                        {
                            result = false;
                        }



                        if (result == true)
                        {
                            if (loopProperties == null)
                                loopProperties = new List<string>();

                            loopProperties.Add(property.Name);

                            RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken);

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

                        JsonReader reader = null;
                        if (currentArrayToken != null && property.Name.Contains("#loopwithincontext"))
                        {
                            strArrayToken = property.Name.Substring(19, property.Name.Length - 20);
                            reader = new JsonTextReader(new StringReader(JsonConvert.SerializeObject(currentArrayToken)));
                        }
                        else
                            reader = new JsonTextReader(new StringReader(inputJson));
                        reader.DateParseHandling = DateParseHandling.None;
                        JToken token = JObject.Load(reader);
                        JToken arrayToken = null;
                        if (strArrayToken.Contains("#"))
                        {
                            int sIndex = strArrayToken.IndexOf("#");
                            string sub1 = strArrayToken.Substring(0, sIndex);

                            int indexOfENdFubction = GetIndexOfFunctionEnd(strArrayToken);

                            if (indexOfENdFubction > sIndex && sIndex > 0)
                            {
                                string sub2 = strArrayToken.Substring(indexOfENdFubction + 1, strArrayToken.Length - indexOfENdFubction - 1);

                                string functionResult = ParseFunction(strArrayToken.Substring(sIndex, indexOfENdFubction - sIndex + 1), inputJson, parentArray, currentArrayToken).ToString();

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

                                RecursiveEvaluate(clonedToken, inputJson, array, elements.Current);

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


                    object newValue = ParseFunction(childToken.Value<string>(), inputJson, parentArray, currentArrayToken);

                    if (newValue != null && newValue.ToString().Contains("\""))
                    {
                        try
                        {
                            JToken newToken = JToken.Parse(newValue.ToString());
                            childToken.Replace(new JValue(newValue));
                        }
                        catch
                        {
                            
                        }
                    }
                    else
                        childToken.Replace(new JValue(newValue));
                }

                if (!isLoop)
                    RecursiveEvaluate(childToken, inputJson, parentArray, currentArrayToken);

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
        private static JToken Copy(string inputString, string inputJson)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string jsonPath = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            JToken token = JObject.Parse(inputJson);

            JToken selectedToken = token.SelectToken(jsonPath);

            return selectedToken;


        }

        #endregion

        #region Delete
        private static string Delete(string inputString)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string path = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);


            return path;


        }

        #endregion

        #region Replace
        private static JToken Replace(string inputString, string inputJson)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string argumentString = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");

            JToken newToken = null;
            object str = ParseFunction(arguments[1], inputJson, null, null);
            if (str != null && str.ToString().Contains("\""))
            {
                newToken = JToken.Parse(str.ToString());

            }
            else
                newToken = str.ToString();

            return newToken;

        }

        private static string GetTokenStringToReplace(string inputString)
        {
            int indexOfStart = inputString.IndexOf("(", 0);
            int indexOfEnd = inputString.LastIndexOf(")");

            string argumentString = inputString.Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

            string[] arguments = argumentString.Split(',');

            if (arguments == null || arguments.Length != 2)
                throw new Exception("#replace needs exactly two arguments - 1. xpath to be replaced, 2. token to replace with.");
            return arguments[0];

        }

        #endregion

        #region ParseFunction

        private static object ParseFunction(string functionString, string inputJson, JArray array, JToken currentArrayElement)
        {
            try
            {
                object output = null;
                functionString = functionString.Trim();
                output = functionString.Substring(1);

                int indexOfStart = output.ToString().IndexOf("(", 0);
                int indexOfEnd = output.ToString().LastIndexOf(")");

                if (indexOfStart == -1 || indexOfEnd == -1)
                    return functionString;

                string functionName = output.ToString().Substring(0, indexOfStart);

                string argumentString = output.ToString().Substring(indexOfStart + 1, indexOfEnd - indexOfStart - 1);

                string[] arguments = GetArguments(argumentString);
                object[] parameters = new object[arguments.Length + 1];

                int i = 0;
                if (arguments != null && arguments.Length > 0)
                {
                    foreach (string argument in arguments)
                    {
                        string trimmedArgument = argument;

                        if (argument.Contains("#"))
                            trimmedArgument = argument.Trim();

                        if (trimmedArgument.StartsWith("#"))
                        {
                            parameters[i] = ParseFunction(trimmedArgument, inputJson, array, currentArrayElement);
                        }
                        else
                            parameters[i] = trimmedArgument;
                        i++;
                    }

                }

                parameters[i] = inputJson;

                if (functionName == "currentvalue" || functionName == "currentindex" || functionName == "lastindex"
                    || functionName == "lastvalue")
                    output = caller("JUST.Transformer", functionName, new object[] { array, currentArrayElement });
                else if (functionName == "currentvalueatpath" || functionName == "lastvalueatpath")
                    output = caller("JUST.Transformer", functionName, new object[] { array, currentArrayElement, arguments[0] });
                else if (functionName == "customfunction")
                    output = CallCustomFunction(parameters);
                else if (functionName == "xconcat" || functionName == "xadd"
                    || functionName == "mathequals" || functionName == "mathgreaterthan" || functionName == "mathlessthan" 
                    || functionName == "mathgreaterthanorequalto"
                    || functionName == "mathlessthanorequalto" || functionName == "stringcontains" || 
                    functionName == "stringequals")
                {
                    object[] oParams = new object[1];
                    oParams[0] = parameters;
                    output = caller("JUST.Transformer", functionName, oParams);
                }
                else
                {
                    if (currentArrayElement != null && functionName != "valueof")
                    {
                        parameters[i] = JsonConvert.SerializeObject(currentArrayElement);
                    }
                    output = caller("JUST.Transformer", functionName, parameters);
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling function : " + functionString + " - " + ex.Message);
            }
        }

        private static object CallCustomFunction(object[] parameters)
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

            return caller(className, functionName, customParameters);

        }

        private static object caller(String myclass, String mymethod, object[] parameters)
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            Type type = Type.GetType(myclass);
            // Create an instance of that type
            //Object obj = Activator.CreateInstance(type);
            // Retrieve the method you are looking for
            MethodInfo methodInfo = type.GetTypeInfo().GetMethod(mymethod);
            // Invoke the method on the instance we created above
            return methodInfo.Invoke(null, parameters);
        }
        #endregion

        #region GetArguments
        private static string[] GetArguments(string functionString)
        {
            bool brackettOpen = false;

            List<string> arguments = null;
            int index = 0;

            int openBrackettCount = 0;
            int closebrackettCount = 0;

            for (int i = 0; i < functionString.Length; i++)
            {
                char currentChar = functionString[i];

                if (currentChar == '(')
                    openBrackettCount++;

                if (currentChar == ')')
                    closebrackettCount++;

                if (openBrackettCount == closebrackettCount)
                    brackettOpen = false;
                else
                    brackettOpen = true;

                if ((currentChar == ',') && (!brackettOpen))
                {
                    if (arguments == null)
                        arguments = new List<string>();

                    if (index != 0)
                        arguments.Add(functionString.Substring(index + 1, i - index - 1));
                    else
                        arguments.Add(functionString.Substring(index, i));
                    index = i;
                }

            }

            if (index > 0)
            {
                arguments.Add(functionString.Substring(index + 1, functionString.Length - index - 1));
            }
            else
            {
                if (arguments == null)
                    arguments = new List<string>();
                arguments.Add(functionString);
            }

            return arguments.ToArray();
        }
        #endregion

        #region Split
        public static IEnumerable<string> SplitJson(string input, string arrayPath)
        {
            JObject inputJObject = JObject.Parse(input);

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
    }
}
