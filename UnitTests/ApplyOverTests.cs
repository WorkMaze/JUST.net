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
        public void ReplaceGraveAccentInJsonPathExpressions()
        {
            var input = "[{ \"result\" : [{ \"code\" : 1, \"description\" : \"EXAMPLE\"},{ \"code\" : 1, \"description\" : \"EXAMPLE\"}]}]";
            var transformer = "{\"data\": \"#applyover({ 'condition': '#valueof(#xconcat($.[0].result[?/(@.description==`EXAMPLE`/)].code))'}, '#valueof($.condition[0])')\"}";
        
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"data\":1}", result);
        }
    }
}
