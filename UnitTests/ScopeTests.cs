using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("Scope")]
    public class ScopeTests
    {
        [Test]
        public void Scope()
        {
            const string input = "{ \"scope1\": { \"val1\": [ 1,2], \"val2\": 2 } }";
            const string transformer = "{ \"result\": { \"#scope($.scope1)\": { \"val1\": \"#valueof($.val1)\", \"val2\": \"#valueof($.val2)\" } } }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"result\":{\"val1\":[1,2],\"val2\":2}}", result);
        }

        [Test]
        public void ScopeAlias()
        {
            const string input = "{ \"scope1\": { \"val1\": [ 1, 2 ], \"val2\": 2 },\"scope2\": { \"val1\": \"val1_scope2\", \"val2\": \"val2_scope2\" } }";
            const string transformer = "{ \"result1\": { \"#scope($.scope1,first)\": { \"val1\": \"#valueof($.val1,first)\", \"val2\": \"#valueof($.val2)\" } }, \"result2\": { \"#scope($.scope2,second)\": { \"val1\": \"#valueof($.val1,second)\", \"val2\": \"#valueof($.val2)\" } } }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"result1\":{\"val1\":[1,2],\"val2\":2},\"result2\":{\"val1\":\"val1_scope2\",\"val2\":\"val2_scope2\"}}", result);
        }

        [Test]
        public void ScopeWithinScope()
        {
            const string input = "{ \"scope1\": { \"val1\": [ 1, 2 ], \"val2\": { \"abc\": 2 } },\"scope2\": { \"val1\": \"val1_scope2\", \"val2\": \"val2_scope2\" } }";
            const string transformer = 
                "{ \"result\": {" + 
                    " \"#scope($.scope1,first)\": { " + 
                        " \"root_scope\": { " + 
                            " \"#scope($.scope2,second,root)\": { " + 
                                " \"val1\": \"#valueof($.val1,first)\", " +
                                " \"val2\": \"#valueof($.val2,second)\", " +
                                " \"val3\": \"#valueof($.val2)\" " + 
                            " } " +
                        " }, " +
                        " \"inner_scope\": { " +
                            " \"#scope($.val2)\": { " +
                                " \"val_abc\": \"#valueof($.abc)\" " + 
                            " } " +
                        " } " +
                    " } " +
                " } " +
            " }";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"result\":{\"root_scope\":{\"val1\":[1,2],\"val2\":\"val2_scope2\",\"val3\":\"val2_scope2\"},\"inner_scope\":{\"val_abc\":2}}}", result);
        }

        [Test]
        public void ScopeLoop()
        {
            const string input = "{ \"scope1\": { \"arr\": [ 1, 2, 3 ], \"val2\": \"val2\" } }";
            const string transformer = "{ \"result\": { \"#scope($.scope1)\": { \"arr\": { \"#loop($.arr)\": \"#currentvalue()\" }, \"val\": \"#valueof($.val2)\" } } } ";

            var result = new JsonTransformer(new JUSTContext() { EvaluationMode = EvaluationMode.Strict}).Transform(transformer, input);

            Assert.AreEqual("{\"result\":{\"arr\":[1,2,3],\"val\":\"val2\"}}", result);
        }
    }
}
