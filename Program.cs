using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JUST;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JUST.NET.Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            string input = File.ReadAllText("Examples/Input.json");

            string transformer = File.ReadAllText("Examples/Transformer_valueof.json"); 
            string transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);


            transformer = File.ReadAllText("Examples/Transformer_valueofarray.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);


            transformer = File.ReadAllText("Examples/Transformer_copy.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_replace.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_delete.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);


            transformer = File.ReadAllText("Examples/Transformer_ifcondition.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_string.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_math.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_aggregate.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_arrayaggregate.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_looping.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_customfunction.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_nestedfunctions.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_xfunctions.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer_Existance.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer.json");
            transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/DataTransformer.xml");
            transformedString = DataTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/DataTransformer.csv");
            transformedString = DataTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);

            transformer = File.ReadAllText("Examples/Transformer.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(input)));
            Console.WriteLine(transformedString);

           

            Console.WriteLine("################################################################################################");
            string inputJson = File.ReadAllText("Examples/ValidationInput.json");
            string schemaJsonX = File.ReadAllText("Examples/SchemaX.json");
            string schemaJsonY = File.ReadAllText("Examples/SchemaY.json");

            string InputToSplit = File.ReadAllText("Examples/InputToSplit.json");

            List<string> outputs = JsonTransformer.SplitJson(InputToSplit, "$.cars.Ford").ToList<string>();

            foreach (string output in outputs)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(output);
            }

            Console.WriteLine("################################################################################################");

            JsonValidator validator = new JsonValidator(inputJson);
            validator.AddSchema("x", schemaJsonX);
            validator.AddSchema("y", schemaJsonY);

            validator.Validate();

            Console.WriteLine("################################################################################################");
            transformer = File.ReadAllText("Examples/Transformer_nestedloop.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(input)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");
            transformer = File.ReadAllText("Examples/Transformer_looptests.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(input)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            string inputSpecial = File.ReadAllText("Examples/InputSpecial.json");
            transformer = File.ReadAllText("Examples/Transformer_customfunctionspecial.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(inputSpecial)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            string inputUnordered = File.ReadAllText("Examples/Input_Unordered.json");
            transformer = File.ReadAllText("Examples/Transform_Unordered.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(inputUnordered)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            string inputUnordered2 = File.ReadAllText("Examples/Input_Unordered_2.json");
            transformer = File.ReadAllText("Examples/Transform_Unordered_2.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(inputUnordered2)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");


            string inputDyn= File.ReadAllText("Examples/InputDynamic.json");
            transformer = File.ReadAllText("Examples/TransformDynamic.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(inputDyn)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            transformer = File.ReadAllText("Examples/Transformer_externalmethods.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JObject.Parse(transformer), JObject.Parse(input)));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            transformer = File.ReadAllText("Examples/Transformer_array.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(transformer, input));
            Console.WriteLine(transformedString);

            Console.WriteLine("################################################################################################");

            transformer = File.ReadAllText("Examples/Transformer_array.json");
            transformedString = JsonConvert.SerializeObject
                (JsonTransformer.Transform(JArray.Parse(transformer), input));
            Console.WriteLine(transformedString);

            Console.ReadLine();
        }
    }
}
