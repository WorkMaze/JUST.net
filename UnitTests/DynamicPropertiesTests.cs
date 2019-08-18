using NUnit.Framework;
using System.Globalization;

namespace JUST.UnitTests
{
    [TestFixture]
    public class DynamicPropertiesTests
    {
        [TestCase("\"is red\"", "\"is red\"")]
        [TestCase(true, "true")]
        [TestCase(12.2, "12.2")]
        [TestCase(null, "null")]
        [TestCase("\"#valueof($.Tree.Branch)\"", "\"leaf\"")]
        public void Eval(object val, object result)
        {
            const string input = "{ \"tree\": { \"branch\": \"leaf\", \"flower\": \"rose\" } }";
            string transformer = "{ \"result\": { \"#eval(#valueof($.tree.flower))\": " + (val?.ToString().ToLower().Replace(",", ".") ?? "null") + " } }";

            var actual = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":{\"rose\":" + result.ToString() + "}}", actual);
        }

        [Test, Category("Loops")]
        public void EvalInsideLoop()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"#eval(#currentvalueatpath($.country.name))\": \"#currentvalueatpath($.country.language)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"Norway\":\"norsk\"},{\"UK\":\"english\"},{\"Sweden\":\"swedish\"}]}", result);
        }
    }
}
