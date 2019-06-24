using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture]
    public class LoopingTests
    {
        [Test]
        public void CurrentValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_value\": \"#currentvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value\":1},{\"current_value\":2},{\"current_value\":3},{\"current_value\":4},{\"current_value\":5}]}", result);
        }

        [Test]
        public void CurrentIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_index\": \"#currentindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_index\":0},{\"current_index\":1},{\"current_index\":2},{\"current_index\":3},{\"current_index\":4}]}", result);
        }

        [Test]
        public void LastIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_index\": \"#lastindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4}]}", result);
        }

        [Test]
        public void LastValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_value\": \"#lastvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5}]}", result);
        }

        [Test]
        public void CurrentValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value_at_path\":\"Norway\"},{\"current_value_at_path\":\"UK\"},{\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void LastValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value_at_path\": \"#lastvalueatpath($.country.language)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"}]}", result);
        }

        [Test]
        public void NestedLooping()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loopwithincontext($.Details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void ContextInputIsJToken()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void GlobalContextInputRestored()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } }, \"root\": \"#valueof($)\" }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}],\"root\":" + ExampleInputs.ObjectArray.Replace(" ", "") + "}", result);
        }

        [Test]
        public void NestedLoopingContextInput()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loopwithincontext($.Details)\": { \"Exists\": \"#exists($.Country)\", \"IsIsland\": \"#ifcondition(#currentvalueatpath($.Country),Iceland,#toboolean(True),#toboolean(False))\", \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"Exists\":true,\"IsIsland\":true,\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"Exists\":true,\"IsIsland\":false,\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }
    }
}
