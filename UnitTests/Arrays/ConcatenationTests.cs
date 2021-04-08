using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture, Category("ArrayInput")]
    public class ConcatenationTests
    {
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

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

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
        public void XconcatArrays(string str1, string str2, string expectedResult)
        {
            var input = $"{{ \"value1\": {str1}, \"value2\": {str2} }}";
            const string transformer = "{ \"result\": \"#xconcat(#valueof($.value1), #valueof($.value2))\" }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [Test]
        public void XconcatMultipleArrays()
        {
            var input = "{ \"value1\": [{ \"prop1\": \"prop1\" },{ \"prop2\": null },{ \"prop3\": \"prop3\" }], \"value2\": [{ \"prop4\": \"prop4\" },{ \"prop5\": null }], \"value3\": [{ \"prop1\": \"prop1\" },{ \"prop2\": null },{ \"prop3\": \"prop3\" }] }";
            const string transformer = "{ \"result\": \"#xconcat(#valueof($.value1), #valueof($.value2), #valueof($.value3))\" }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"prop1\":\"prop1\"},{\"prop2\":null},{\"prop3\":\"prop3\"},{\"prop4\":\"prop4\"},{\"prop5\":null},{\"prop1\":\"prop1\"},{\"prop2\":null},{\"prop3\":\"prop3\"}]}", result);
        }
    }
}
