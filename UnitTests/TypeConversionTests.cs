using System.Globalization;
using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class TypeConversionTests
    {
        [TestCase("\"true\"", true)]
        [TestCase("\"false\"", false)]
        [TestCase("0", false)]
        [TestCase("123", true)]
        [TestCase("-456", true)]
        public void ToBooleanConvertion(string typedValue, bool expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#toboolean(#valueof($.value))\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult.ToString().ToLower()}}}", result);
        }

        [TestCase("true", "True")]
        [TestCase("false", "False")]
        [TestCase("0", "0")]
        [TestCase("123", "123")]
        [TestCase("-456", "-456")]
        [TestCase("1.23", "1<decimalSeparator>23")]
        [TestCase("-4.56", "-4<decimalSeparator>56")]
        public void ToStringConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#tostring(#valueof($.value))\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            expectedResult = expectedResult.Replace("<decimalSeparator>", decimalSeparator);
            Assert.AreEqual($"{{\"result\":\"{expectedResult}\"}}", result);
        }

        [TestCase("\"123\"", "123")]
        [TestCase("\"-456\"", "-456")]
        [TestCase("\"0\"", "0")]
        [TestCase("1.01", "1")]
        [TestCase("1.23", "1")]
        [TestCase("-4.56", "-5")]
        [TestCase("true", "1")]
        [TestCase("false", "0")]
        public void ToIntegerConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#tointeger(#valueof($.value))\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [TestCase("\"0\"", "0.0")]
        [TestCase("\"1.01\"", "1.01")]
        [TestCase("123", "123.0")]
        [TestCase("-123", "-123.0")]
        public void ToDecimalConvertion(string typedValue, string expectedResult)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            const string transformer = "{ \"result\": \"#todecimal(#valueof($.value))\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [TestCase("null", "null", "null")]
        [TestCase("[]", "[]", "[]")]
        [TestCase("[]", "null", "[]")]
        [TestCase("null", "[]", "[]")]
        [TestCase("[ \"string1\" ]", "null", "[\"string1\"]")]
        [TestCase("null", "[\"string2\"]", "[\"string2\"]")]
        [TestCase("[{ \"prop1\": null }]", "null", "[{\"prop1\":null}]")]
        [TestCase("null", "[{ \"prop2\": null }]", "[{\"prop2\":null}]")]
        [TestCase("[{ \"prop1\": null }]", "[{ \"prop2\": null }]", "[{\"prop1\":null},{\"prop2\":null}]")]
        [TestCase("[{ \"prop1\": \"prop1\" },{ \"prop2\": null },{ \"prop3\": \"prop3\" }]", "[{ \"prop4\": \"prop4\" },{ \"prop5\": null }]", "[{\"prop1\":\"prop1\"},{\"prop2\":null},{\"prop3\":\"prop3\"},{\"prop4\":\"prop4\"},{\"prop5\":null}]")]
        public void ConcatArrays(string str1, string str2, string expectedResult)
        {
            var input = $"{{ \"value1\": {str1}, \"value2\": {str2} }}";
            const string transformer = "{ \"result\": \"#concat(#valueof($.value1), #valueof($.value2))\" }";

            var result = new JsonTransformer(new JUSTContext {EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [Test]
        public void ConcatMultipleArrays()
        {
            var input = "{ \"value1\": [{ \"prop1\": \"prop1\" },{ \"prop2\": null },{ \"prop3\": \"prop3\" }], \"value2\": [{ \"prop4\": \"prop4\" },{ \"prop5\": null }], \"value3\": [{ \"prop1\": \"prop1\" },{ \"prop2\": null },{ \"prop3\": \"prop3\" }] }";
            const string transformer = "{ \"result\": \"#concat(#concat(#valueof($.value1), #valueof($.value2)), #valueof($.value3))\" }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"prop1\":\"prop1\"},{\"prop2\":null},{\"prop3\":\"prop3\"},{\"prop4\":\"prop4\"},{\"prop5\":null},{\"prop1\":\"prop1\"},{\"prop2\":null},{\"prop3\":\"prop3\"}]}", result);
        }
    }
}
