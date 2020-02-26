using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JUST
{
    internal class ExpressionHelper
    {
        private const string FunctionAndArgumentsRegex = "^#(.+?)[(](.*)[)]$";

        internal static bool TryParseFunctionNameAndArguments(string input, out string functionName, out string arguments)
        {
            var match = new Regex(FunctionAndArgumentsRegex).Match(input);
            functionName = match.Success ? match.Groups[1].Value : input;
            arguments = match.Success ? match.Groups[2].Value : null;
            return match.Success;
        }

        internal static string[] GetArguments(string functionString)
        {
            if (string.IsNullOrEmpty(functionString))
            {
                return new string[0]; 
            }

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

                brackettOpen = openBrackettCount != closebrackettCount;

                if ((currentChar == ',') && (!brackettOpen))
                {
                    if (arguments == null)
                        arguments = new List<string>();
                    
                    arguments.Add(index != 0 ?
                        functionString.Substring(index + 1, i - index - 1) :
                        functionString.Substring(index, i));
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
    }
}
