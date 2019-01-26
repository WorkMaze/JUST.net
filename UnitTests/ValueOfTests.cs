using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("ValueOf")]
    public class ValueOfTests
    {
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

        [Test]
        public void Array()
        {
            const string transformer = "{ \"root\": { \"array\": { \"fullarray\": \"#valueof($.x)\" } }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"fullarray\":[{\"v\":{\"a\":\"a1,a2,a3\",\"b\":\"1\",\"c\":\"10\"}},{\"v\":{\"a\":\"b1,b2\",\"b\":\"2\",\"c\":\"20\"}},{\"v\":{\"a\":\"c1,c2,c3\",\"b\":\"3\",\"c\":\"30\"}}]}}}", result);
        }

        [Test]
        public void ArrayElement()
        {
            const string transformer = "{ \"root\": { \"array\": { \"arrayelement\": \"#valueof($.x[0])\" } }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"arrayelement\":{\"v\":{\"a\":\"a1,a2,a3\",\"b\":\"1\",\"c\":\"10\"}}}}}", result);
        }

        [Test]
        public void ArrayElementSpecificField()
        {
            const string transformer = "{ \"root\": { \"array\": { \"specific_field\": \"#valueof($.x[1].v.a)\" } }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"specific_field\":\"b1,b2\"}}}", result);
        }

        [Test]
        public void ReadmeTest()
        {
            const string transformer = "{\"root\": {\"menu1\": \"#valueof($.menu.popup.menuitem[?(@.value=='New')].onclick)\", \"menu2\": \"#valueof($.menu.popup.menuitem[?(@.value=='Open')].onclick)\"}}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"root\":{\"menu1\":{\"action\":\"CreateNewDoc()\"},\"menu2\":\"OpenDoc()\"}}", result);
        }
    }
}