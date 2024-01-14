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

        [Test]
        public void LoopsWithLookInTransformed()
        {
            const string input = "{ \"topLevelItems\": [ { \"name\": \"item1\", \"type\": \"someType\", \"selectable\": false, \"itemArray\": [ { \"property1\": \"item1_value1\", \"property2\": \"item1_value2\" } ] }, { \"name\": \"item2\", \"type\": \"someType\", \"selectable\": true, \"itemArray\": [ { \"property1\": \"item2_value1\", \"property2\": \"item2_value2\" } ] } ]}";
            const string transformer = "{ \"filteredItems\": [ \"#valueof($.topLevelItems[?(@.selectable == true)])\" ], \"summary\": { \"#loop($.filteredItems)\": { \"name\": \"#currentvalueatpath($.name)\" } }, \"aliasedItems\": \"#valueof($..filteredItems)\" }";

             JUSTContext context = new JUSTContext { EvaluationMode = EvaluationMode.Strict | EvaluationMode.LookInTransformed };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"filteredItems\":[{\"name\":\"item2\",\"type\":\"someType\",\"selectable\":true,\"itemArray\":[{\"property1\":\"item2_value1\",\"property2\":\"item2_value2\"}]}],\"summary\":[{\"name\":\"item2\"}],\"aliasedItems\":[{\"name\":\"item2\",\"type\":\"someType\",\"selectable\":true,\"itemArray\":[{\"property1\":\"item2_value1\",\"property2\":\"item2_value2\"}]}]}", result);
        }
    }
}