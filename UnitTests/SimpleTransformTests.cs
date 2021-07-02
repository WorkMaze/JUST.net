using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class SimpleTransformTests
    {
        [Test]
        public void String()
        {
            var input = "{\"Food\": {\"Desserts\": {\"item\": [{\"name\": \"carrot cake\",\"price\": 5},{\"name\": \"ice cream\",\"price\": 10}]}}}";
            var transformer = "\"abc\"";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("\"abc\"", result);
        }

        [Test]
        public void StringWithFunction()
        {
            var input = "{\"Food\": {\"Desserts\": {\"item\": [{\"name\": \"carrot cake\",\"price\": 5},{\"name\": \"ice cream\",\"price\": 10}]}}}";
            var transformer = "\"#valueof($.Food.Desserts.item)\"";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("[{\"name\":\"carrot cake\",\"price\":5},{\"name\":\"ice cream\",\"price\":10}]", result);
        }

        [Test]
        public void ReturnInput()
        {
            var input = "{\"Food\": {\"Desserts\": {\"item\": [{\"name\": \"carrot cake\",\"price\": 5},{\"name\": \"ice cream\",\"price\": 10}]}}}";
            var transformer = "\"#valueof($)\"";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"Food\":{\"Desserts\":{\"item\":[{\"name\":\"carrot cake\",\"price\":5},{\"name\":\"ice cream\",\"price\":10}]}}}", result);
        }

        [Test]
        public void SimpleArrayElement()
        {
            var input = "{\"Food\": {\"Desserts\": {\"item\": [{\"name\": \"carrot cake\",\"price\": 5},{\"name\": \"ice cream\",\"price\": 10}]}}}";
            var transformer = "[ \"#valueof($.Food.Desserts.item[*].name)\" ]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("[\"carrot cake\",\"ice cream\"]", result);
        }
    }
}
