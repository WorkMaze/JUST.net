using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("IfCondition")]
    public class IfConditionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SetupTest()
        {
            Assert.Pass();
        }

        [Test]
        public void PrimitiveFirstTrueConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(true,#valueof($.boolean),truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"truevalue\"}", result);
        }

        [Test]
        public void FnFirstTrueConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.boolean),true,truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"truevalue\"}", result);
        }
    }
}
