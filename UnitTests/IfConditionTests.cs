using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("IfCondition")]
    public class IfConditionTests
    {
        [Test]
        public void PrimitiveFirstTrueConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(true,#valueof($.boolean),truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"truevalue\"}", result);
        }

        public void PrimitiveFirstFalseConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(dummy,#valueof($.string),truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"falsevalue\"}", result);
        }

        [Test]
        public void FnFirstTrueConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.boolean),true,truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"truevalue\"}", result);
        }

        [Test]
        public void FnFirstFalseConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.integer),555,truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"falsevalue\"}", result);
        }

        [Test]
        public void FnBothTrueConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true, \"same_integer\": 123 }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.integer),#valueof($.same_integer),truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"truevalue\"}", result);
        }

        [Test]
        public void FnBothFalseConditionStringResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true, \"same_integer\": 123 }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.string),#valueof($.same_integer),truevalue,falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"falsevalue\"}", result);
        }

        [Test]
        public void TrueFnResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true, \"same_integer\": 123 }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.integer),#valueof($.same_integer),#valueof($.boolean),falsevalue)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":true}", result);
        }

        [Test]
        public void FalseFnResult()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true, \"other_integer\": 1235 }";
            const string transformer = "{ \"result\": \"#ifcondition(#valueof($.integer),#valueof($.other_integer),truevalue,#valueof($.other_integer))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":1235}", result);
        }

        [Test]
        public void ReadmeTest()
        {
            const string input = "{ \"menu\": { \"id\" : \"github\", \"repository\" : \"JUST\" } }";
            const string transformer = "{ \"ifconditiontesttrue\": \"#ifcondition(#valueof($.menu.id),github,#valueof($.menu.repository),fail)\", \"ifconditiontestfalse\": \"#ifcondition(#valueof($.menu.id),xml,#valueof($.menu.repository),fail)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"ifconditiontesttrue\":\"JUST\",\"ifconditiontestfalse\":\"fail\"}", result);
        }
    }
}
