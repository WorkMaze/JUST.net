using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture]
    public class GroupFunctionsTests
    {
        [Test]
        public void GroupBySingleElement()
        {
            const string transformer = "{ \"Result\": \"#grouparrayby($.Forest,type,all)\" }";
            const string input = "{ \"Forest\": [ { \"type\": \"Mammal\", \"qty\": 1, \"name\": \"Hippo\" }, { \"type\": \"Bird\", \"qty\": 2, \"name\": \"Sparrow\" }, { \"type\": \"Amphibian\", \"qty\": 300, \"name\": \"Lizard\" }, { \"type\": \"Bird\", \"qty\": 3, \"name\": \"Parrot\" }, { \"type\": \"Mammal\", \"qty\": 1, \"name\": \"Elephant\" }, { \"type\": \"Mammal\", \"qty\": 10, \"name\": \"Dog\" } ] }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"Result\":[{\"type\":\"Mammal\",\"all\":[{\"qty\":1,\"name\":\"Hippo\"},{\"qty\":1,\"name\":\"Elephant\"},{\"qty\":10,\"name\":\"Dog\"}]},{\"type\":\"Bird\",\"all\":[{\"qty\":2,\"name\":\"Sparrow\"},{\"qty\":3,\"name\":\"Parrot\"}]},{\"type\":\"Amphibian\",\"all\":[{\"qty\":300,\"name\":\"Lizard\"}]}]}", result);
        }

        [Test]
        public void GroupByMultipleElements()
        {
            const string transformer = "{ \"Result\": \"#grouparrayby($.Vehicle,type:company,all)\" }";
            const string input = "{ \"Vehicle\": [ { \"type\": \"air\", \"company\": \"Boeing\", \"name\": \"airplane\" }, { \"type\": \"air\", \"company\": \"Concorde\", \"name\": \"airplane\" }, { \"type\": \"air\", \"company\": \"Boeing\", \"name\": \"Chopper\" }, { \"type\": \"land\", \"company\": \"GM\", \"name\": \"car\" }, { \"type\": \"sea\", \"company\": \"Viking\", \"name\": \"ship\" }, { \"type\": \"land\", \"company\": \"GM\", \"name\": \"truck\" } ] }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"Result\":[{\"type\":\"air\",\"company\":\"Boeing\",\"all\":[{\"name\":\"airplane\"},{\"name\":\"Chopper\"}]},{\"type\":\"air\",\"company\":\"Concorde\",\"all\":[{\"name\":\"airplane\"}]},{\"type\":\"land\",\"company\":\"GM\",\"all\":[{\"name\":\"car\"},{\"name\":\"truck\"}]},{\"type\":\"sea\",\"company\":\"Viking\",\"all\":[{\"name\":\"ship\"}]}]}", result);
        }
    }
}
