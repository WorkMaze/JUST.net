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

        [Test]
        public void LoopingRoot()
        {
            var input = "[{\"Id\":1,\"Fields\":{\"Email\":{\"Name\":\"Email\",\"FieldType\":3,\"Value\":\"test1@test.com\"}},\"CreatedAt\":\"2021-10-07T13:40:14.813Z\"},{\"Id\":2,\"Fields\":{\"Email\":{\"Name\":\"Email\",\"FieldType\":3,\"Value\":\"test2@test.com\"}},\"CreatedAt\":\"2021-10-07T13:44:24.48Z\"},{\"Id\":3,\"Fields\":{\"Email\":{\"Name\":\"Email\",\"FieldType\":3,\"Value\":\"test3@test.com\"}},\"CreatedAt\":\"2021-10-07T13:45:09.417Z\"}]";
            var transformer = "[{\"#loop($)\":{\"#eval(#currentvalueatpath($.Id))\":\"#currentvalueatpath($.Fields['Email'].Value)\"}}]";

            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("[{\"1\":\"test1@test.com\"},{\"2\":\"test2@test.com\"},{\"3\":\"test3@test.com\"}]", result);
        }
    }
}
