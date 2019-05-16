using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class DynamicPropertiesTests
    {
        [Test]
        public void Eval()
        {
            const string input = "{ \"Tree\": { \"Branch\": \"leaf\", \"Flower\": \"Rose\" } }";
            const string transformer = "{ \"Result\": { \"#eval(#valueof($.Tree.Flower))\": \"is red\" } }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"Result\":{\"Rose\":\"is red\"}}", result);
        }
    }
}
