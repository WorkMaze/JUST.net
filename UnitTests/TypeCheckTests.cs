using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("Type"), Category("TypeCheck")]
    public class TypeCheckTests
    {
        [TestCase("0", true)]
        [TestCase("1.23", true)]
        [TestCase("true", false)]
        [TestCase("\"abc\"", false)]
        [TestCase("[ \"abc\", \"xyz\" ]", false)]
        public void IsNumber(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = "{ \"result\": \"#isnumber(#valueof($.value))\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }

        [TestCase("0", false)]
        [TestCase("1.23", false)]
        [TestCase("true", true)]
        [TestCase("\"abc\"", false)]
        [TestCase("[ \"abc\", \"xyz\" ]", false)]
        public void IsBoolean(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = "{ \"result\": \"#isboolean(#valueof($.value))\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }

        [TestCase("0", false)]
        [TestCase("1.23", false)]
        [TestCase("true", false)]
        [TestCase("\"abc\"", true)]
        [TestCase("[ \"abc\", \"xyz\" ]", false)]
        public void IsString(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = "{ \"result\": \"#isstring(#valueof($.value))\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }

        [TestCase("0", false)]
        [TestCase("1.23", false)]
        [TestCase("true", false)]
        [TestCase("\"abc\"", false)]
        [TestCase("[ \"abc\", \"xyz\" ]", true)]
        public void IsArray(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = "{ \"result\": \"#isarray(#valueof($.value))\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }
    }
}
