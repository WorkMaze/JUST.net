using NUnit.Framework;

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

            var actual = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":{\"rose\":" + result.ToString() + "}}", actual);
        }

        [Test, Category("Loops")]
        public void EvalInsideLoop()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"#eval(#currentvalueatpath($.country.name))\": \"#currentvalueatpath($.country.language)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"Norway\":\"norsk\"},{\"UK\":\"english\"},{\"Sweden\":\"swedish\"}]}", result);
        }

        [Test, Category("Loops")]
        public void MultipleEvals()
        {
            const string input = "{\"people\": [{ \"name\": \"Jim\", \"number\": \"0123-4567-8888\" }, { \"name\": \"John\", \"number\": \"0134523-4567-8910\" }]}";
            const string transformer = "{ \"root\": { \"#loop($.people)\": { \"#eval(#xconcat(name, #add(#currentindex(),1)))\": \"#currentvalueatpath($.name)\", \"#eval(#xconcat(number, #add(#currentindex(),1)))\": \"#currentvalueatpath($.number)\" } } }";

            var context = new JUSTContext();
            context.EvaluationMode = EvaluationMode.Strict;
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"root\":[{\"name1\":\"Jim\",\"number1\":\"0123-4567-8888\"},{\"name2\":\"John\",\"number2\":\"0134523-4567-8910\"}]}", result);
        }

        [Test]
        public void TypedValuesInsideEval()
        {
            const string input = "{ \"a\": [{ \"name\": \"prop1\", \"val\": 1 }, { \"name\": \"prop2\", \"val\": 2 }] }";
            string transformer = "{ \"a\": { \"#loop($.a)\": { \"#eval(#currentvalueatpath($.name))\": \"#currentvalueatpath($.val)\" } } }";

            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"a\":[{\"prop1\":1},{\"prop2\":2}]}", result);
        }

        [Test]
        public void EvalWithObjectInside()
        {
            var input = "{ \"sections\": [ { \"id\": \"first\", \"label\": \"First section\" }, { \"id\": \"second\", \"label\": \"Second section\" } ] }";
            var transformer = "{ \"areas\": { \"#loop($.sections)\": { \"#eval(#currentvalueatpath($.id))\": { \"description\": \"#currentvalueatpath($.label)\" } } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"areas\":[{\"first\":{\"description\":\"First section\"}},{\"second\":{\"description\":\"Second section\"}}]}", result);
        }
    }
}
