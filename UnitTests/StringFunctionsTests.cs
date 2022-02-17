using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture]
    public class StringFunctionsTests
    {
        [Test]
        public void FirstIndexOf()
        {
            var transformer = "{ \"stringresult\": { \"firstindexofand\": \"#firstindexof(#valueof($.stringref),and)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"firstindexofand\":6}}", result);
        }

        [Test]
        public void LastIndexOf()
        {
            var transformer = "{ \"stringresult\": { \"lastindexofand\": \"#lastindexof(#valueof($.stringref),and)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"lastindexofand\":21}}", result);
        }

        [Test]
        public void Substring()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),8,10)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"substring\":\"dveryunuas\"}}", result);
        }

        [Test]
        public void SubstringFallbackToDefault()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),100,100)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"substring\":null}}", result);
        }

        [Test]
        public void SubstringStrictError()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),100,100)\" }}";

            Assert.Throws<Exception>(() => new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, ExampleInputs.StringRef));
        }

        [Test]
        public void Concat()
        {
            var transformer = "{ \"stringresult\": { \"concat\": \"#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"stringresult\":{\"concat\":\"csvpopup\"}}", result);
        }

        [Test]
        public void Equals()
        {
            var transformer = "{ \"stringresult\": { \"stringequals\": \"#stringequals(#valueof($.d[0]),one)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringequals\":true}}", result);
        }

        [Test]
        public void EqualsCaseSensitive()
        {
            var transformer = "{ \"stringresult\": { \"stringequals\": \"#stringequals(#valueof($.d[0]),oNe)\" }}";

            var context = new JUSTContext() { EvaluationMode = EvaluationMode.Strict };
            var result = new JsonTransformer(context).Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringequals\":false}}", result);
        }

        [Test]
        public void EqualsOnNull()
        {
            var transformer = "{ \"stringresult\": { \"stringequals\": \"#stringequals(#valueof($.not_there),one)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringequals\":false}}", result);
        }

        [Test]
        public void Contains()
        {
            var transformer = "{ \"stringresult\": { \"stringcontains\": \"#stringcontains(#valueof($.d[0]),n)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringcontains\":true}}", result);
        }

        [Test]
        public void ContainsCaseSensitive()
        {
            var transformer = "{ \"stringresult\": { \"stringcontains\": \"#stringcontains(#valueof($.d[0]),N)\" }}";

            var context = new JUSTContext() { EvaluationMode = EvaluationMode.Strict };
            var result = new JsonTransformer(context).Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringcontains\":false}}", result);
        }

        [Test]
        public void ContainsOnNull()
        {
            var transformer = "{ \"stringresult\": { \"stringcontains\": \"#stringcontains(#valueof($.not_there),n)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringcontains\":false}}", result);
        }

        [Test]
        public void StringEmpty()
        {
            var input = "{ \"empty\": \"\", \"not_empty\": \"not empty\" }";
            var transformer = "{ \"test_empty\": \"#ifcondition(#valueof($.empty),#stringempty(),is empty,not empty)\", \"test_not_empty\": \"#ifcondition(#valueof($.not_empty),#stringempty(),empty,is not empty)\" }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"test_empty\":\"is empty\",\"test_not_empty\":\"is not empty\"}", result);
        }
    }
}
