using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("ArrayInput")]
    public class ArrayInputTests
    {
        [Test]
        public void SingleResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": \"#valueof([?(@.gender=='F')].name)\" }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict })
                .Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"Person 2\"}", result);
        }

        [Test]
        public void Iterate()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": { \"name\": \"#currentvalueatpath($.name)\" } } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict })
                .Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"name\":\"Person 1\"},{\"name\":\"Person 3\"}]}", result);
        }
    }
}
