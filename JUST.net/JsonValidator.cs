using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JUST
{
    public class JsonValidator
    {
        private Dictionary<string, string> schemaCollection;
        private string schemaNoPrefix;
        private string inputJsonString;

        public JsonValidator(string inputJson)
        {
            inputJsonString = inputJson; 
        }

        public void AddSchema(string prefix,string schemaJson)
        {
            if (prefix == string.Empty)
                schemaNoPrefix = schemaJson;
            else
            {
                if (schemaCollection == null)
                    schemaCollection = new Dictionary<string, string>();
                schemaCollection.Add(prefix, schemaJson);
            }
        }

        public void Validate()
        {
            List<string> errors = null;

            SchemaValidationEventHandler handler = new SchemaValidationEventHandler(HandleEvent);
            if (!string.IsNullOrEmpty(schemaNoPrefix))
            {
                JObject xSchemaToken = JsonConvert.DeserializeObject<JObject>(schemaNoPrefix);
                JSchema schema = JSchema.Parse(JsonConvert.SerializeObject(xSchemaToken));
                JObject json = JsonConvert.DeserializeObject<JObject>(inputJsonString);
                try
                {
                    json.Validate(schema, handler);
                }
                catch(Exception e)
                {
                    if (errors == null)
                        errors = new List<string>();
                    errors.Add(e.Message);
                }
            }
            if(schemaCollection != null)
            {
                foreach(KeyValuePair<string,string> schemaPair in schemaCollection)
                {

                    JObject xSchemaToken = JsonConvert.DeserializeObject<JObject>(schemaPair.Value);
                    PrefixKey(xSchemaToken, schemaPair.Key);
                    JSchema schema = JSchema.Parse(JsonConvert.SerializeObject(xSchemaToken));
                    JObject json = JsonConvert.DeserializeObject<JObject>(inputJsonString);
                    try
                    {
                        json.Validate(schema, handler);
                    }
                    catch (Exception e)
                    {
                        if (errors == null)
                            errors = new List<string>();
                        errors.Add(e.Message);
                    }
                }
            }

            if (errors != null)
            {
                throw new Exception(string.Join(" AND ", errors.ToArray()));
            }


        }

        private static void PrefixKey(JObject jo, string prefix)
        {
            foreach (JProperty jp in jo.Properties().ToList())
            {
                if (jp.Value.Type == JTokenType.Object)
                {
                    PrefixKey((JObject)jp.Value, prefix);
                }
                else if (jp.Value.Type == JTokenType.Array)
                {
                    foreach (JToken child in jp.Value)
                    {
                        if (child.Type == JTokenType.Object)
                        {
                            PrefixKey((JObject)child, prefix);
                        }
                    }
                }

                if (jp.Name.ToLower() != "type" && jp.Name.ToLower() != "title"
                    && jp.Name.ToLower() != "description" && jp.Name.ToLower() != "default"
                    && jp.Name.ToLower() != "enum" && jp.Name.ToLower() != "properties"
                    && jp.Name.ToLower() != "additionalproperties")
                {
                    string name = prefix + "." + jp.Name;
                    jp.Replace(new JProperty(name, jp.Value));
                }

            }
        }


        private static void HandleEvent(object sender, SchemaValidationEventArgs args)
        {
            throw new Exception( args.Message);
           
        }

    }
}
