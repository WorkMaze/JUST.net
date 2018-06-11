using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JUST
{
    public class LoopContents
    {
        public string Evaluated { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
    }

    public class Parser
    {

        public static string Parse(string input, string loop)
        {
            int startIndex = 0, index = 0;

            while ((index = input.IndexOf('#', startIndex)) != -1)
            {
                int endElementIndex = input.IndexOf('"', index);
                int startingElementIndex = input.LastIndexOf('"', startIndex);


                if (endElementIndex > index)
                {
                    startIndex = endElementIndex + 1;
                    string functionString = input.Substring(index, endElementIndex - index);


                    if (functionString.Trim().Contains("#loop"))
                    {
                        LoopContents content = FindLoopContents(input, endElementIndex, functionString);
                        Console.WriteLine(content.Evaluated);

                        StringBuilder builder = new StringBuilder(input);
                        builder.Remove(content.Start, content.End - content.Start + 1);
                        builder.Insert(content.Start, content.Evaluated);
                        input = builder.ToString();

                        startIndex = content.Start + content.Evaluated.Length;// content.End;

                        //StringBuilder builder = new StringBuilder(input);
                        //builder.Remove(startingElementIndex, content.End - startingElementIndex);
                        //builder.Insert(startingElementIndex, "[" + content.Evaluated + "]");
                        //input = builder.ToString();

                        //startIndex = startingElementIndex + content.Evaluated.Length + 2;
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder(input);
                        builder.Remove(index, endElementIndex - index);
                        string evaluatedFunction = EvaluateFunction(functionString, loop);
                        builder.Insert(index, evaluatedFunction);
                        input = builder.ToString();

                        startIndex = index + evaluatedFunction.Length;
                    }

                    //Console.WriteLine(functionString);
                }
                else
                    break;
            }

            return input;


        }

        public static string EvaluateFunction(string functionString, string loop)
        {
            return loop == null ? "SAY_WHAT" : loop + "_YES";
        }

        public static LoopContents FindLoopContents(string input, int startIndex, string loop)
        {

            LoopContents contents = new LoopContents();

            string remainingString = input.Substring(startIndex);

            string result = string.Empty;

            int indexOfColon = remainingString.IndexOf(':');


            char searchCharacter = '{';
            bool searchCharaterInitialized = false;

            int opened = 0;
            int closed = 0;


            if (indexOfColon != -1)
            {

                remainingString = remainingString.Substring(indexOfColon + 1);

                int startCharIndex = indexOfColon;

                int endCharIndex = indexOfColon;

                int i = 0;
                foreach (char c in remainingString)
                {
                    if (c == '"')
                    {
                        if (!searchCharaterInitialized)
                        {
                            searchCharaterInitialized = true;
                            searchCharacter = '"';
                            startCharIndex = i;
                            opened++;
                        }
                        else
                        {
                            if (searchCharaterInitialized && (searchCharacter == '"'))
                                closed++;
                            endCharIndex = i;
                        }
                    }
                    if (c == '[')
                    {
                        if (!searchCharaterInitialized)
                        {
                            searchCharaterInitialized = true;
                            searchCharacter = '[';
                            startCharIndex = i;
                        }
                        if (searchCharacter == '[')
                            opened++;
                    }
                    if (c == '{')
                    {
                        if (!searchCharaterInitialized)
                        {
                            searchCharaterInitialized = true;
                            searchCharacter = '{';
                            startCharIndex = i;
                        }
                        if (searchCharacter == '{')
                            opened++;
                    }
                    if (c == ']')
                    {
                        if (searchCharaterInitialized && (searchCharacter == '['))
                        {
                            closed++;
                            endCharIndex = i;
                        }
                    }
                    if (c == '}')
                    {
                        if (searchCharaterInitialized && (searchCharacter == '{'))
                        {
                            closed++;
                            endCharIndex = i;
                        }
                    }

                    if (closed > 0 && closed >= opened)
                        break;

                    i++;
                }

                result = remainingString.Substring(startCharIndex, endCharIndex - startCharIndex + 1);

                contents.Start = startIndex + startCharIndex + indexOfColon + 1;
                contents.End = startIndex + endCharIndex + indexOfColon + 1;
            }

            contents.Evaluated = result;

            if (contents.Start == 0)
                contents.Start = startIndex + 1;
            if (contents.End == 0)
                contents.End = startIndex + 1;

            contents.Evaluated = Parse(contents.Evaluated, loop);

            return contents;
        }


    }
}