using NJsonSchema;
using NJsonSchema.Validation;
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
        private readonly string inputJsonString;

        public JsonValidator(string inputJson)
        {
            inputJsonString = inputJson; 
        }

        public void AddSchema(string prefix,string schemaJson)
        {
            if (string.IsNullOrEmpty(prefix))
                schemaNoPrefix = schemaJson;
            else
            {
                if (schemaCollection == null)
                    schemaCollection = new Dictionary<string, string>();
                schemaCollection.Add(prefix, schemaJson);
            }
        }

        public async Task Validate()
        {
            List<string> errors = new List<string>();

            if (!string.IsNullOrEmpty(schemaNoPrefix))
            {
                errors = await Validate(schemaNoPrefix, inputJsonString);
            }
            if (schemaCollection != null)
            {
                foreach (KeyValuePair<string, string> schemaPair in schemaCollection)
                {
                    errors.AddRange(await Validate(schemaPair.Value, inputJsonString));
                }
            }

            if (errors.Count > 0)
            {
                throw new Exception(string.Join(" AND ", errors.ToArray()));
            }
        }

        private async Task<List<string>> Validate(string schemaJson, string inputJson)
        {
            List<string> result = new List<string>();
            JsonSchema xSchemaToken = await JsonSchema.FromJsonAsync(schemaJson);
            ICollection<ValidationError> schemaErrors = xSchemaToken.Validate(inputJson);
            foreach (var error in schemaErrors)
            {
                result.Add(error.ToString());
            }
            return result;
        }
    }
}
