using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace JUST.UnitTests
{
    [TestFixture, Category("SchemaValidator")]
    public class JsonValidatorTests
    {
        [Test]
        public async Task SchemaOk()
        {
            const string input = "{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" }";
            const string schema = "{ \"$schema\": \"https://json-schema.org/draft/2020-12/schema\", \"$id\": \"https://just.net\", \"title\": \"JUST Test\", \"description\": \"JUST test schema\", \"type\": \"object\", \"properties\": { \"id\": { \"description\": \"JUST Test\", \"type\": \"integer\" } }, \"required\": [ \"id\" ]}";
            var v = new JsonValidator(input);
            v.AddSchema(null, schema);

            await v.Validate();

            Assert.Pass();
        }

        [Test, Category("SchemaValidator")]
        public void MissingRequiredProp()
        {
            const string input = "{ \"name\": \"Person 1\", \"gender\": \"M\" }";
            const string schema = "{ \"$schema\": \"https://json-schema.org/draft/2020-12/schema\", \"$id\": \"https://just.net\", \"title\": \"JUST Test\", \"description\": \"JUST test schema\", \"type\": \"object\", \"properties\": { \"id\": { \"description\": \"JUST Test\", \"type\": \"integer\" } }, \"required\": [ \"id\" ]}";
            var v = new JsonValidator(input);
            v.AddSchema(null, schema);

            var result = Assert.ThrowsAsync<Exception>(async () => await v.Validate());

            Assert.AreEqual("PropertyRequired: #/id", result.Message);
        }
    }
}
