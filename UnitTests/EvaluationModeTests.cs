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
    }
}