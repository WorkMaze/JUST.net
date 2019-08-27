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
            var transformer = "{ \"result\": \"#applyover({ 'result': { '#loop($.values)': { 'test': '#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),true,yes,no)' } } }, '#exists($.result[?(@.test=='yes')])')\", \"after_result\": \"#valueof($.d[0])\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);

            Assert.AreEqual("{\"result\":true,\"after_result\":\"one\"}", result);
        }
    }
}
