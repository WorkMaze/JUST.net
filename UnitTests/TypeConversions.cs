using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class TypeConversions
    {
        [TestCase("\"true\"", true)]
        [TestCase("\"false\"", false)]
        [TestCase("0", false)]
        [TestCase("1", true)]
        [TestCase("2", true)]
        public void ToBooleanConvertion(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#toboolean(#valueof($.value))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }

        [TestCase("true", "True")]
        [TestCase("false", "False")]
        [TestCase("0", "0")]
        [TestCase("1.01", "1,01")]
        public void ToStringConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#tostring(#valueof($.value))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":\"{expectedResult}\"}}", result);
        }

        [TestCase("\"1\"", "1")]
        [TestCase("\"-1\"", "-1")]
        [TestCase("\"0\"", "0")]
        [TestCase("1.01", "1")]
        [TestCase("1.55", "2")]
        public void ToIntegerConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#tointeger(#valueof($.value))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [TestCase("\"0\"", "0.0")]
        [TestCase("\"1.01\"", "1.01")]
        public void ToDecimalConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#todecimal(#valueof($.value))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [TestCase("0.00154", "0.00", 2)]
        [TestCase("0.01554", "0.02", 2)]
        [TestCase("0.66489", "1.0", 0)]
        public void Round(string typedValue, string expectedResult, int decimalPlaces)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = $"{{ \"result\": \"#round(#valueof($.value),{decimalPlaces})\" }}";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }
    }
}
