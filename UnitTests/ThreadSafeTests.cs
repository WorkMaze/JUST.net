using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace JUST.UnitTests
{
    [TestFixture]
    public class ThreadSafeTests
    {
        [Test]
        public void TestTransforms1()
        {
            var input = File.ReadAllText("Inputs/thread_safe_input.json");
            const string transformer = "{ \"list\": { \"#loop($.list)\": { \"id\": \"#currentindex()\", \"name\": \"#concat(#currentvalueatpath($.title), #currentvalueatpath($.name))\", \"contact\": \"#currentvalueatpath($.contacts[?(@.is_default==true)])\", \"address\": \"#currentvalueatpath($.addresses[0])\" } }, \"applyover\": \"#applyover({ 'first_title': '#valueof($.list[0].title)'/, 'second_address': '#valueof($.list[1].addresses[1])'/, 'first_contact': '#valueof($.list[0].contacts[0])' }, '#valueof($.second_address.street)')\" }";
            const string expect = "{\"list\":[{\"id\":0,\"name\":\"Mr.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":987654321,\"is_default\":true},\"address\":{\"street\":\"Some Street\",\"number\":1,\"city\":\"Some City\",\"postal_code\":1234}},{\"id\":1,\"name\":\"Mrs.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":111111111111111,\"is_default\":true},\"address\":{\"street\":\"Street Who?\",\"number\":11,\"city\":\"City Who?\",\"postal_code\":1111}}],\"applyover\":\"Other Street Who?\"}";

            JsonTransformer jsonTransformer = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict });

            string transformResult = jsonTransformer.Transform(transformer, input);
            Assert.AreEqual(expect, transformResult);
        }

        [Test]
        public void TestTransforms2()
        {
            var input = File.ReadAllText("Inputs/thread_safe_input.json");
            const string transformer = "{ \"result\": { \"name\": \"#concat(#valueof($.title), #valueof($.name))\", \"contact\": \"#valueof($.contacts[?(@.is_default==true)])\", \"address\": \"#valueof($.addresses[0])\" } }";
            const string expect = "{\"result\":{\"name\":\"Mr.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":987654321,\"is_default\":true},\"address\":{\"street\":\"Some Street\",\"number\":1,\"city\":\"Some City\",\"postal_code\":1234}}}";

            JsonTransformer jsonTransformer = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict });

            string transformResult = jsonTransformer.Transform(transformer, input);
            Assert.AreEqual(expect, transformResult);
        }

        [Test]
        public void ThreadSafe()
        {
            var input = File.ReadAllText("Inputs/thread_safe_input.json");
            string[] transformers =
            {
                "{ \"list\": { \"#loop($.list)\": { \"id\": \"#currentindex()\", \"name\": \"#concat(#currentvalueatpath($.title), #currentvalueatpath($.name))\", \"contact\": \"#currentvalueatpath($.contacts[?(@.is_default==true)])\", \"address\": \"#currentvalueatpath($.addresses[0])\" } }, \"applyover\": \"#applyover({ 'first_title': '#valueof($.list[0].title)'/, 'second_address': '#valueof($.list[1].addresses[1])'/, 'first_contact': '#valueof($.list[0].contacts[0])' }, '#valueof($.second_address.street)')\" }",
                "{ \"result\": { \"name\": \"#concat(#valueof($.title), #valueof($.name))\", \"contact\": \"#valueof($.contacts[?(@.is_default==true)])\", \"address\": \"#valueof($.addresses[0])\" } }"
            };
            string[] expects =
            {
                "{\"list\":[{\"id\":0,\"name\":\"Mr.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":987654321,\"is_default\":true},\"address\":{\"street\":\"Some Street\",\"number\":1,\"city\":\"Some City\",\"postal_code\":1234}},{\"id\":1,\"name\":\"Mrs.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":111111111111111,\"is_default\":true},\"address\":{\"street\":\"Street Who?\",\"number\":11,\"city\":\"City Who?\",\"postal_code\":1111}}],\"applyover\":\"Other Street Who?\"}",
                "{\"result\":{\"name\":\"Mr.Smith\",\"contact\":{\"type\":\"mobile\",\"number\":987654321,\"is_default\":true},\"address\":{\"street\":\"Some Street\",\"number\":1,\"city\":\"Some City\",\"postal_code\":1234}}}",
            };

            var jsonTransformer = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict });

            ParallelLoopResult result = Parallel.For(0, 100, i =>
            {
                JToken transformResult = jsonTransformer.Transform(JObject.Parse(transformers[i % 2]), input);
                Assert.AreEqual(JObject.Parse(expects[i % 2]), transformResult);
            });

            Assert.IsTrue(result.IsCompleted);
        }
    }
}
