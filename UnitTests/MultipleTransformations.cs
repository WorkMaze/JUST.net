using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class MultipleTransformations
    {
        [Test]
        public void MultipleTransformsScalarResult()
        {
            const string input = "{\"d\": [ \"one\", \"two\", \"three\" ], \"values\": [ \"z\", \"c\", \"n\" ]}";
            const string transformer = 
                "{ \"result\": " +
                    "{ \"#transform($)\": [ " + 
                        "{ \"condition\": { \"#loop($.values)\": { \"test\": \"#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),True,yes,no)\" } } }, " +
                        "{ \"intermediate_transform\": \"#valueof($.condition)\" }," +
                          "\"#exists($.intermediate_transform[?(@.test=='yes')])\" ] } }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"result\":true}", result);
        }

        [Test]
        public void MultipleTransformsObjectResult()
        {
            const string input = "{\"d\": [ \"one\", \"two\", \"three\" ], \"values\": [ \"z\", \"c\", \"n\" ]}";
            const string transformer = 
                "{ \"object\": " +
                    "{ \"#transform($)\": [ " + 
                        "{ \"condition\": { \"#loop($.values)\": { \"test\": \"#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),True,yes,no)\" } } }, " +
                        "{ \"intermediate_transform\": \"#valueof($.condition)\" }," +
                        "{ \"result\": \"#exists($.intermediate_transform[?(@.test=='yes')])\" } ] } }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"object\":{\"result\":true}}", result);
        }

        [Test]
        public void MultipleTransformsOverSelectedToken()
        {
            const string input = "{ \"select\": {\"d\": [ \"one\", \"two\", \"three\" ], \"values\": [ \"z\", \"c\", \"n\" ]} }";
            const string transformer = 
                "{ \"select_token\": " +
                    "{ \"#transform($.select)\": [ " + 
                        "{ \"condition\": { \"#loop($.values)\": { \"test\": \"#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),True,yes,no)\" } } }, " +
                        "{ \"intermediate_transform\": \"#valueof($.condition)\" }," +
                        "{ \"result\": \"#exists($.intermediate_transform[?(@.test=='yes')])\" } ] } }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"select_token\":{\"result\":true}}", result);
        }

        [Test]
        public void MultipleTransformsWithinLoop()
        {
            const string input = "{ \"select\": [{ \"d\": [ \"one\", \"two\", \"three\" ], \"values\": [ \"z\", \"c\", \"n\" ] }, { \"d\": [ \"four\", \"five\", \"six\" ], \"values\": [ \"z\", \"c\", \"n\" ] }] }";
            const string transformer = 
                "{ \"loop\": {" +
                    " \"#loop($.select,selectLoop)\": { " +
                            "\"#transform($)\": [ " + 
                                "{ \"condition\": { \"#loop($.values)\": { \"test\": \"#ifcondition(#stringcontains(#currentvalueatpath($.d[0],selectLoop),#currentvalue()),True,yes,no)\" } } }, " +
                                "{ \"intermediate_transform\": \"#valueof($.condition)\" }," +
                                "{ \"result\": \"#exists($.intermediate_transform[?(@.test=='yes')])\" } ] " +
                        " } } }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"loop\":[{\"result\":true},{\"result\":false}]}", result);
        }
    }
}