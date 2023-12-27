using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture, Category("EvaluationMode")]
    public class EvaluationModeTests
    {
        [Test]
        public void Combined()
        {
            const string transformer = "{ \"#\": [ \"#copy($.menu)\" ], \"popup\": { \"menuitem\": [] } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.FallbackToDefault | EvaluationMode.AddOrReplaceProperties }).Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"popup\":{\"menuitem\":[],\"submenuitem\":\"CloseSession()\"},\"id\":{\"file\":\"csv\"},\"value\":{\"Window\":\"popup\"}}", result);
        }

        [Test]
        public void MultipleLevelLookInTransformed()
        {
            const string input = "{ \"id\": \"123\"  }";
            const string transformer = "{ \"level1\": { \"quoteId\": \"#valueof($.id)\", \"notResolved\": \"#valueof($.quoteId)\", \"level2\": { \"resolved1\": \"#valueof($..quoteId)\", \"level3\": { \"resolved2\": \"#valueof($..quoteId)\" } } } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.LookInTransformed | EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"level1\":{\"quoteId\":\"123\",\"notResolved\":null,\"level2\":{\"resolved1\":\"123\",\"level3\":{\"resolved2\":\"123\"}}}}", result);
        }
    }
}