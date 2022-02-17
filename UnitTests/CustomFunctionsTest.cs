using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JUST.UnitTests
{
    [TestFixture, Category("CustomFunctions")]
    public class CustomFunctionsTests
    {
        [Test]
        public void XmlTest()
        {
            string inputSpecial = File.ReadAllText("Inputs/Input_Customfunction_Nestedresult.json");
            string transformer = File.ReadAllText("Inputs/Transformer_customfunctionnestedresult.json");
            string result = JsonConvert.SerializeObject
                (new JsonTransformer().Transform(JObject.Parse(transformer), JObject.Parse(inputSpecial)));
            Assert.AreEqual("{\"Seasons\":[[[[\"2017\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]],[[\"2018\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]],[[\"2019\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]]]]}", result);
        }

        [Test]
        public void RegisteredCustomFunction()
        {
            const string input = "{ \"ExcessQuoteAmendments\": [{ \"Name\": \"test_name\", \"Attributes\": [{ \"CoverageDataType\": \"test_coverage\", \"Value\": \"test_value\"}, { }] }]";
            string transformer = "{ \"ExcessFormsList\": { \"#loop($.ExcessQuoteAmendments)\": { \"AttributesList\": { \"#loop($.Attributes)\": { \"Value\":\"#aliascustomfunction(#currentvalueatpath($.Value), #currentvalueatpath($.CoverageDataType))\" }}}}}";
            var context = new JUSTContext();
            context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "CheckNullParameters", "aliascustomfunction");

            var actual = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"ExcessFormsList\":[{\"AttributesList\":[{\"Value\":\"test_value\"},{\"Value\":null}]}]}", actual);
        }

        [Test]
        public void AssemblyDefined()
        {
            const string input = "{ \"ExcessQuoteAmendments\": [{ \"Name\": \"test_name\", \"Attributes\": [{ \"CoverageDataType\": \"test_coverage\", \"Value\": \"test_value\"}, { }] }]";
            string transformer = "{ \"ExcessFormsList\": { \"#loop($.ExcessQuoteAmendments)\": { \"AttributesList\": { \"#loop($.Attributes)\": { \"Value\":\"#ExternalMethods::ExternalMethods.ExternalClass::CheckNullParameters(#currentvalueatpath($.Value), #currentvalueatpath($.CoverageDataType))\" }}}}}";

            var actual = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"ExcessFormsList\":[{\"AttributesList\":[{\"Value\":\"test_value\"},{\"Value\":null}]}]}", actual);
        }

        [Test]
        public void WithoutAssembly()
        {
            const string input = "{ \"ExcessQuoteAmendments\": [{ \"Name\": \"test_name\", \"Attributes\": [{ \"CoverageDataType\": \"test_coverage\", \"Value\": \"test_value\"}, { }] }]";
            string transformer = "{ \"ExcessFormsList\": { \"#loop($.ExcessQuoteAmendments)\": { \"AttributesList\": { \"#loop($.Attributes)\": { \"Value\":\"#JUST.UnitTests.CustomFunctionsTests::CheckNullParameters(#currentvalueatpath($.Value), #currentvalueatpath($.CoverageDataType))\" }}}}}";

            var actual = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"ExcessFormsList\":[{\"AttributesList\":[{\"Value\":\"test_value\"},{\"Value\":null}]}]}", actual);
        }

        public object CheckNullParameters(object val1, object val2)
        {
            return val1 ?? val2;
        }
    }
}