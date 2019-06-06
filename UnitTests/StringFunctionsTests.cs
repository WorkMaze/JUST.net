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

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"firstindexofand\":6}}", result);
        }

        [Test]
        public void LastIndexOf()
        {
            var transformer = "{ \"stringresult\": { \"lastindexofand\": \"#lastindexof(#valueof($.stringref),and)\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"lastindexofand\":21}}", result);
        }

        [Test]
        public void Substring()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),8,10)\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"substring\":\"dveryunuas\"}}", result);
        }

        [Test]
        public void SubstringFallbackToDefault()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),100,100)\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringRef);

            Assert.AreEqual("{\"stringresult\":{\"substring\":null}}", result);
        }

        [Test]
        public void SubstringStrictError()
        {
            var transformer = "{ \"stringresult\": { \"substring\": \"#substring(#valueof($.stringref),100,100)\" }}";

            Assert.Throws<Exception>(() => JsonTransformer.Transform(transformer, ExampleInputs.StringRef, new JUSTContext { EvaluationMode = EvaluationMode.Strict }));
        }

        [Test]
        public void Concat()
        {
            var transformer = "{ \"stringresult\": { \"concat\": \"#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"stringresult\":{\"concat\":\"csvpopup\"}}", result);
        }

        [Test]
        public void Equals()
        {
            var transformer = "{ \"stringresult\": { \"stringequals\": \"#stringequals(#valueof($.d[0]),one)\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringequals\":true}}", result);
        }

        [Test]
        public void Contains()
        {
            var transformer = "{ \"stringresult\": { \"stringcontains\": \"#stringcontains(#valueof($.d[0]),n)\" }}";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"stringresult\":{\"stringcontains\":true}}", result);
        }
    }
}
