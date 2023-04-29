using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class ApplyOverTests
    {
        [Test]
        public void ApplyOverInputRetake()
        {
            var input = "{\"d\": [ \"one\", \"two\", \"three\" ], \"values\": [ \"z\", \"c\", \"n\" ]}";
            var transformer = "{ \"result\": \"#applyover({ 'condition': { '#loop($.values)': { 'test': '#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),True,yes,no)' } } }, '#exists($.condition[?(@.test=='yes')])')\", \"after_result\": \"#valueof($.d[0])\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":true,\"after_result\":\"one\"}", result);
        }

        [Test]
        public void ObjectTransformationResult()
        {
            const string input = "{ \"data\": [ { \"saleStatus\": 1, \"priority\": \"normal\", \"other\": \"one\" }, { \"saleStatus\": 2, \"priority\": \"high\", \"other\": \"two\" }, { \"saleStatus\": 1, \"priority\": \"normal\", \"other\": \"three\" } ] }";
            const string transformer = "{ \"result\": \"#applyover({ 'temp': '#grouparrayby($.data,saleStatus:priority,all)' }, { '#loop($.temp)': { 'count': '#length($.all)' } })\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"count\":2},{\"count\":1}]}", result);
        }

        [Test]
        public void ArrayTransformationResult()
        {
            const string input = "{ \"data\": [ { \"saleStatus\": 1, \"priority\": \"normal\", \"other\": \"one\" }, { \"saleStatus\": 2, \"priority\": \"high\", \"other\": \"two\" }, { \"saleStatus\": 1, \"priority\": \"normal\", \"other\": \"three\" } ] }";
            const string transformer = "{ \"result\": \"#applyover({ 'temp': { '#loop($.data)': { 'field': '#currentvalueatpath($.other)' } } }, { '#loop($.temp)': '#currentvalueatpath($.field)' })\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[\"one\",\"two\",\"three\"]}", result);
        }

        [Test]
        public void InsideLoopInputIsLoopElement()
        {
            var input = "{ \"sections\": [ { \"id\": \"first\", \"label\": \"First section\" }, { \"id\": \"second\", \"label\": \"Second section\" } ] }";
            var transformer = "{ \"areas\": { \"#loop($.sections)\": { \"#eval(#currentvalueatpath($.id))\": \"#applyover({ 'description': '#valueof($.label)' }, '#valueof($)')\" } } }";

            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);
            
            Assert.AreEqual("{\"areas\":[{\"first\":{\"description\":\"First section\"}},{\"second\":{\"description\":\"Second section\"}}]}", result);
        }

        [Test]
        public void InsideNestedLoopsWithAlias()
        {
            var input = "{ \"headers\": [ { \"id\": \"first\", \"label\": \"First header\", \"sections\": [{ \"title\": \"first section first header\" },{ \"title\": \"second section first header\" }] }, { \"id\": \"second\", \"label\": \"Second header\", \"sections\": [{ \"title\": \"first section second header\" },{ \"title\": \"second section second header\" }] }] }";
            var transformer = "{ \"areas\": { \"#loop($.headers,outside_alias)\": { \"#eval(#currentvalueatpath($.id))\": { \"#loop($.sections,inside_alias)\": { \"section\": \"#applyover({ 'description': '#valueof($.label)' }, '#valueof($)',outside_alias)\" } } } } }";

            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"areas\":[{\"first\":[{\"section\":{\"description\":\"First header\"}},{\"section\":{\"description\":\"First header\"}}]},{\"second\":[{\"section\":{\"description\":\"Second header\"}},{\"section\":{\"description\":\"Second header\"}}]}]}", result);
        }   
    }
}
