using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JUST
{
    public class DataTransformer
    {
        private static string Parse(string transformer, string inputJson, JArray array, JToken currentArrayElement)
        {
            int startIndex = 0, index = 0;

            while ((startIndex < transformer.Length) && (index = transformer.IndexOf('#', startIndex)) != -1)
            {
                string functionString = GetFunctionString(transformer, index);


                if (functionString != null)
                {
                    if (functionString.Contains("loop"))
                    {
                        string loopArgsInclusive = GetLoopArguments(index, transformer, true);
                        string loopArgs = GetLoopArguments(index, transformer, false);

                        string evaluatedFunction = (string)EvaluateFunction(functionString, inputJson, array, currentArrayElement, loopArgs);
                                               

                        StringBuilder builder = new StringBuilder(transformer);
                        builder.Remove(index-1, loopArgsInclusive.Length+1);
                        builder.Insert(index-1, evaluatedFunction);
                        transformer = builder.ToString();

                        startIndex = index + evaluatedFunction.Length;
                       
                    }
                    else
                    {

                        string evaluatedFunction = (string)EvaluateFunction(functionString, inputJson, array, currentArrayElement, null);

                        if (!string.IsNullOrEmpty(evaluatedFunction))
                        {

                            StringBuilder builder = new StringBuilder(transformer);
                            builder.Remove(index, functionString.Length);
                            builder.Insert(index, evaluatedFunction);
                            transformer = builder.ToString();

                            startIndex = index + evaluatedFunction.Length;
                        }
                    }
                }
                else
                    break;
            }

            return transformer;
        }

        public static string Transform(string transformer, string inputJson)
        {

            return Parse(transformer, inputJson,null,null);

        }

        private static string GetLoopArguments(int startIdex, string input,bool inclusive)
        {
            string loopArgs = string.Empty;


            int openBrackettCount = 0;
            int closebrackettCount = 0;


            int bStartIndex = 0;
            int bEndIndex = 0;
            for (int i = startIdex; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (currentChar == '{')
                {
                    if (openBrackettCount == 0)
                        bStartIndex = i;

                    openBrackettCount++;
                }

                if (currentChar == '}')
                {
                    bEndIndex = i;
                    closebrackettCount++;
                }

               

                if (openBrackettCount > 0 && openBrackettCount == closebrackettCount)
                {
                    if(!inclusive)
                        loopArgs = input.Substring(bStartIndex, bEndIndex - bStartIndex + 1);
                    else
                        loopArgs = input.Substring(startIdex, bEndIndex - startIdex + 1);
                    break;
                }

            }

            if (inclusive && loopArgs == string.Empty)
                return "#loop";

            return loopArgs;
        }

        private static string EvaluateFunction(string functionString, string inputJson, JArray array, JToken currentArrayElement,
                    string loopArgumentString)
        {
            string output = null;

            functionString = functionString.Trim().Substring(1);

            int indexOfStart = functionString.IndexOf("(");

            if (indexOfStart != -1)
            {

                string functionName = functionString.Substring(0, indexOfStart);

                string argumentString = functionString.Substring(indexOfStart + 1, functionString.Length - indexOfStart - 2);

                string[] arguments = GetArguments(argumentString);

                string[] parameters = new string[arguments.Length + 1];

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
                            parameters[i] = (string)EvaluateFunction(trimmedArgument, inputJson, array, currentArrayElement, loopArgumentString);
                        }
                        else
                            parameters[i] = trimmedArgument;
                        i++;
                    }

                }

                parameters[i] = inputJson;

                if (functionName == "loop")
                {
                    output = GetLoopResult(parameters, loopArgumentString);
                }
                else if (functionName == "currentvalue" || functionName == "currentindex" || functionName == "lastindex"
                    || functionName == "lastvalue")
                    output = (string)caller("JUST.Transformer", functionName, new object[] { array, currentArrayElement });
                else if (functionName == "currentvalueatpath" || functionName == "lastvalueatpath")
                    output = (string)caller("JUST.Transformer", functionName, new object[] { array, currentArrayElement, arguments[0] });
                else if (functionName == "customfunction")
                    output = (string)CallCustomFunction(parameters);
                else if (functionName == "xconcat" || functionName == "xadd" || functionName == "mathequals" || functionName == "mathgreaterthan" || functionName == "mathlessthan"
                    || functionName == "mathgreaterthanorequalto"
                    || functionName == "mathlessthanorequalto" || functionName == "stringcontains" ||
                    functionName == "stringequals")
                {
                    object[] oParams = new object[1];
                    oParams[0] = parameters;
                    output = caller("JUST.Transformer", functionName, oParams).ToString();
                }
                else
                    output = caller("JUST.Transformer", functionName, parameters).ToString();
            }
            return output;
        }


        private static string GetLoopResult(string[] parameters,string loopArgumentString)
        {
            string returnString = string.Empty;

            if (parameters.Length < 2)
                throw new Exception("Incorrect number of parameters for function #Loop");

            string input = parameters[1];

            if (parameters.Length == 3)
                input = parameters[2];

            JToken token = JsonConvert.DeserializeObject<JObject>(input);
            JToken selectedToken = token.SelectToken(parameters[0]);

            if (selectedToken.Type != JTokenType.Array)
                throw new Exception("The JSONPath argument inside a #loop function must be an Array");

            JArray selectedArr = selectedToken as JArray;

            string seperator = Environment.NewLine;

            if (parameters.Length == 3)
                seperator = parameters[1];

            foreach (JToken arrToken in selectedToken.Children())
            {
                string parsedrecord = Parse(loopArgumentString, input, selectedArr, arrToken);

                returnString += parsedrecord.Substring(1, parsedrecord.Length - 2);
                returnString += seperator;
            }

            return returnString;
        }

        private static string GetFunctionString(string input, int startIndex)
        {
            string functionString = string.Empty;

            int brackettOpenCount = 0;
            int brackettClosedCount = 0;

            for(int i = startIndex; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(')
                    brackettOpenCount++;
                if (c == ')')
                    brackettClosedCount++;

                if( brackettClosedCount > 0 && brackettClosedCount == brackettOpenCount)
                {
                    functionString = input.Substring(startIndex, i - startIndex + 1);
                    break;
                }
            }

            return functionString;
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



        private static string[] GetArguments(string argumentString)
        {
            bool brackettOpen = false;

            List<string> arguments = null;
            int index = 0;

            int openBrackettCount = 0;
            int closebrackettCount = 0;

            for (int i = 0; i < argumentString.Length; i++)
            {
                char currentChar = argumentString[i];

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
                        arguments.Add(argumentString.Substring(index + 1, i - index - 1));
                    else
                        arguments.Add(argumentString.Substring(index, i));
                    index = i;
                }

            }

            if (index > 0)
            {
                arguments.Add(argumentString.Substring(index + 1, argumentString.Length - index - 1));
            }
            else
            {
                if (arguments == null)
                    arguments = new List<string>();
                arguments.Add(argumentString);
            }

            return arguments.ToArray();
        }

        
    }
}
