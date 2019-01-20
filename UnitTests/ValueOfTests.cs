using JUST;
using NUnit.Framework;

namespace Just.net.Tests
{
    [TestFixture, Category("ValueOf")]
    public class ValueOfTests
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
        public void String()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof($.string)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"some words\"}", result);
        }

        [Test]
        public void Integer()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof($.integer)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":123}", result);
        }

        [Test]
        public void Boolean()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof($.boolean)\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":true}", result);
        }

        [Test]
        public void Nested()
        {
            const string input = "{ \"string\": \"$.integer\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof(#valueof($.string))\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":123}", result);
        }
    }
}