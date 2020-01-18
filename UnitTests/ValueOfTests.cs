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

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"some words\"}", result);
        }

        [Test]
        public void Integer()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof($.integer)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":123}", result);
        }

        [Test]
        public void Boolean()
        {
            const string input = "{ \"string\": \"some words\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof($.boolean)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":true}", result);
        }

        [Test]
        public void Nested()
        {
            const string input = "{ \"string\": \"$.integer\", \"integer\": 123, \"boolean\": true }";
            const string transformer = "{ \"result\": \"#valueof(#valueof($.string))\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":123}", result);
        }

        [Test]
        public void Array()
        {
            const string transformer = "{ \"root\": { \"array\": { \"fullarray\": \"#valueof($.x)\" } }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"fullarray\":[{\"v\":{\"a\":\"a1,a2,a3\",\"b\":\"1\",\"c\":\"10\"}},{\"v\":{\"a\":\"b1,b2\",\"b\":\"2\",\"c\":\"20\"}},{\"v\":{\"a\":\"c1,c2,c3\",\"b\":\"3\",\"c\":\"30\"}}]}}}", result);
        }

        [Test]
        public void ArrayEmpty()
        {
            const string transformer = "{ \"array\": \"#valueof($.x)\" }";
            const string input = "{ \"x\": [ { \"y\": [] } ] }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"array\":[{\"y\":[]}]}", result);
        }

        [Test]
        public void ArrayElement()
        {
            const string transformer = "{ \"root\": { \"array\": { \"arrayelement\": \"#valueof($.x[0])\" } }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"arrayelement\":{\"v\":{\"a\":\"a1,a2,a3\",\"b\":\"1\",\"c\":\"10\"}}}}}", result);
        }

        [Test]
        public void ArrayElementSpecificField()
        {
            const string transformer = "{ \"root\": { \"array\": { \"specific_field\": \"#valueof($.x[1].v.a)\" } }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ArrayX);

            Assert.AreEqual("{\"root\":{\"array\":{\"specific_field\":\"b1,b2\"}}}", result);
        }

        [Test]
        public void MultidimensionArrays()
        {
            const string input = "{\"paths\": [{\"points\": {\"coordinates\": [[ 106.621279, 10.788109 ],[ 106.621672, 10.787869 ],[ 106.621992, 10.787717 ]]}}]}";
            const string transformer = "{ \"result\": \"#valueof($.paths)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"points\":{\"coordinates\":[[106.621279,10.788109],[106.621672,10.787869],[106.621992,10.787717]]}}]}", result);
        }

        [Test]
        public void PrimitiveElementsArray()
        {
            const string input = "{\"root\": [\"elem1\",\"elem2\"]}";
            const string transformer = "{ \"result\": \"#valueof($.root)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":[\"elem1\",\"elem2\"]}", result);
        }

        [Test]
        public void MultipleLevelArray()
        {
            const string input = "{\"outer_array\": [ { \"inner_array\": [\"elem1\",\"elem2\" ] } ] }";
            const string transformer = "{ \"result\": \"#valueof($.outer_array..inner_array)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":[\"elem1\",\"elem2\"]}", result);
		}
        
		[Test]
		public void DoublePrecision()
        {
            const string input = "{ \"Latitude\": 38.978378, \"Longitude\": -122.032861 }";
            const string transformer = "{ \"LatitudeInDecimalDegrees\": \"#valueof($.Latitude)\", \"LongitudeInDecimalDegrees\": \"#valueof($.Longitude)\" }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"LatitudeInDecimalDegrees\":38.978378,\"LongitudeInDecimalDegrees\":-122.032861}", result);
        }
    }
}