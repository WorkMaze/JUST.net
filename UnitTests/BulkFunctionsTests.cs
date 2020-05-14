using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture]
    public class BulkFunctionsTests
    {
        [Test]
        public void Copy()
        {
            const string transformer = "{ \"#\": [ \"#copy($.menu.id)\", \"#copy($.menu.value)\" ] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"file\":\"csv\",\"Window\":\"popup\"}", result);
        }

        [Test]
        public void ReplaceWithPrimitiveValue()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace($.menu.id,#valueof($.menu.value.Window))\"] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":\"popup\",\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void ReplaceWithObject()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace($.menu.id,#valueof($.menu.value))\"] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":{\"Window\":\"popup\"},\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void Delete()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#delete($.menu.popup)\" ] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":{\"file\":\"csv\"},\"value\":{\"Window\":\"popup\"}}}", result);
        }

        [Test]
        public void CopyNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy(#xconcat($,.menu,.id))\" ] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"file\":\"csv\"}", result);
        }

        [Test]
        public void ReplaceNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace(#valueof(#concat($.,path)),#valueof($.menu.value))\"] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"path\":\"$.menu.id\",\"boolean\":true,\"menu\":{\"id\":{\"Window\":\"popup\"},\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void DeleteNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#delete(#valueof($.path))\" ] }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"path\":\"$.menu.id\",\"boolean\":true,\"menu\":{\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void CopyInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => JsonTransformer.Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Invalid jsonPath for #copy!", result.Message);
        }

        [Test]
        public void ReplaceInvalidArgumentNumber()
        {
            const string transformer = "{ \"#\": [ \"#replace(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<Exception>(() => JsonTransformer.Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Function #replace needs two arguments - 1. jsonPath to be replaced, 2. token to replace with.", result.Message);
        }

        [Test]
        public void ReplaceInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#replace(#valueof($.boolean), #valueof($.menu.value))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => JsonTransformer.Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Invalid jsonPath for #replace!", result.Message);
        }

        [Test]
        public void DeleteInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#delete(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => JsonTransformer.Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Invalid jsonPath for #delete!", result.Message);
        }
    }
}
