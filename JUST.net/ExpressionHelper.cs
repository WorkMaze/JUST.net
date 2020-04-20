using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace JUST
{
    internal class ExpressionHelper
    {
        private const string EscapeChar = "/"; //do not use backslash, it is already the escape char in JSON
        private const string FunctionAndArgumentsRegex = "^#(.+?)[(](.*)[)]$";

        internal static bool TryParseFunctionNameAndArguments(string input, out string functionName, out string arguments)
        {
            var match = new Regex(FunctionAndArgumentsRegex).Match(input);
            functionName = match.Success ? match.Groups[1].Value : input;
            arguments = match.Success ? match.Groups[2].Value : null;
            return match.Success;
        }

        internal static string[] SplitArguments(string args)
        {
            var commaSplit = Regex.Split(args, $"(?<![\\{EscapeChar}]),").ToList();
            for (int i = 0; i < commaSplit.Count;)
            {
                var arg = commaSplit[i];
                var openBrackets = Regex.Matches(arg, $"(?<![\\{EscapeChar}])\\(");
                var closeBrackets = Regex.Matches(arg, $"(?<![\\{EscapeChar}])\\)");
                var openQuotes = arg.StartsWith("'");
                var closeQuotes = arg.EndsWith("'");
                if (openBrackets.Count == closeBrackets.Count && !(openQuotes && !closeQuotes))
                {
                    if (openQuotes)
                    {
                        commaSplit[i] = commaSplit[i].Trim('\'');
                    }
                    else
                    {
                        commaSplit[i] = !IsFunction(commaSplit[i]) ? 
                            Unescape(commaSplit[i]) :
                            commaSplit[i];
                    }
                    i++;
                    continue;
                }

                if (commaSplit.Count > i + 1)
                {
                    commaSplit[i] += "," + commaSplit.ElementAt(i + 1);
                    commaSplit.RemoveAt(i + 1);
                }
                else
                {
                    if (openBrackets.Count > 0)
                    {
                        throw new Exception("Expected closing round brackets.");
                    }
                    if (openQuotes)
                    {
                        throw new Exception("Expected closing single quote.");
                    }

                    commaSplit[i] = !IsFunction(commaSplit[i]) ?
                            Unescape(commaSplit[i]) :
                            commaSplit[i];
                    i++;
                }

            }
            return commaSplit.ToArray();
        }

        private static string Unescape(string str)
        {
            /* TODO use Regex to unnescape to avoid sequencial replaces without back/forward lookups
            * Example: '\\)' -> replace '\\' = '\)' -> replace '\)' = ')' 
            */
            //Regex.Replace(commaSplit[i], $"(?<![\\(])\\{EscapeChar}\\((?![\\)])", expr => "(")
            //Regex.Replace(commaSplit[i], $"(?<![\\(])\\{EscapeChar}\\)(?![\\)])", expr => ")")
            return str
                .Replace($"{EscapeChar}{EscapeChar}", EscapeChar)
                .Replace($"{EscapeChar}(", "(")
                .Replace($"{EscapeChar})", ")")
                .Replace($"{EscapeChar},", ",")
                .Replace($"{EscapeChar}'", "'");
        }

        internal static bool IsFunction(string val)
        {
            return Regex.IsMatch(val, "^\\s*#");
        }

        internal static string UnescapeSharp(string val)
        {
            return Regex.Replace(val, "^\\s*\\/#", "#");
        }
    }
}
