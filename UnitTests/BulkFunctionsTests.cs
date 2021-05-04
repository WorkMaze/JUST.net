using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture, Category("Bulk")]
    public class BulkFunctionsTests
    {
        [Test]
        public void Copy()
        {
            const string transformer = "{ \"#\": [ \"#copy($.menu.id)\", \"#copy($.menu.value)\" ] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"file\":\"csv\",\"Window\":\"popup\"}", result);
        }

        [Test]
        public void ReplaceWithPrimitiveValue()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace($.menu.id,#valueof($.menu.value.Window))\"] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":\"popup\",\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}],\"submenuitem\":\"CloseSession()\"}}}", result);
        }

        [Test]
        public void ReplaceWithObject()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace($.menu.id,#valueof($.menu.value))\"] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":{\"Window\":\"popup\"},\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}],\"submenuitem\":\"CloseSession()\"}}}", result);
        }

        [Test]
        public void Delete()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#delete($.menu.popup)\" ] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"menu\":{\"id\":{\"file\":\"csv\"},\"value\":{\"Window\":\"popup\"}}}", result);
        }

        [Test]
        public void CopyNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy(#xconcat($,.menu,.id))\" ] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"file\":\"csv\"}", result);
        }

        [Test]
        public void ReplaceNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#replace(#valueof(#concat($.,path)),#valueof($.menu.value))\"] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"path\":\"$.menu.id\",\"boolean\":true,\"menu\":{\"id\":{\"Window\":\"popup\"},\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void DeleteNestedArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#delete(#valueof($.path))\" ] }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MenuNested);

            Assert.AreEqual("{\"path\":\"$.menu.id\",\"boolean\":true,\"menu\":{\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", result);
        }

        [Test]
        public void CopyInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#copy(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => new JsonTransformer().Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual($"Invalid path for #copy: '#valueof($.boolean)' resolved to null!", result.Message);
        }

        [Test]
        public void ReplaceInvalidArgumentNumber()
        {
            const string transformer = "{ \"#\": [ \"#replace(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<Exception>(() => new JsonTransformer().Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Function #replace needs at least two arguments - 1. path to be replaced, 2. token to replace with.", result.Message);
        }

        [Test]
        public void ReplaceInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#replace(#valueof($.boolean), #valueof($.menu.value))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => new JsonTransformer().Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Invalid path for #replace: '#valueof($.boolean)' resolved to null!", result.Message);
        }

        [Test]
        public void DeleteInvalidArgument()
        {
            const string transformer = "{ \"#\": [ \"#delete(#valueof($.boolean))\" ] }";

            var result = Assert.Throws<ArgumentException>(() => new JsonTransformer().Transform(transformer, ExampleInputs.Menu));

            Assert.AreEqual("Invalid path for #delete: '#valueof($.boolean)' resolved to null!", result.Message);
        }

        [Test]
        public void CopyAddUnknownProperty()
        {
            const string input = "{ \"unknown-property\": \"value\", \"known-property\": { \"unknown-sub-property1\": \"value1\", \"unknown-sub-property2\": \"value2\", \"unknown-sub-propertyN\": \"valueN\" } }";
            const string transformer = "{ \"#\": [ \"#copy($)\" ], \"added-property\": 1 }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"added-property\":1,\"unknown-property\":\"value\",\"known-property\":{\"unknown-sub-property1\":\"value1\",\"unknown-sub-property2\":\"value2\",\"unknown-sub-propertyN\":\"valueN\"}}", result);
        }

        [Test]
        public void CopyAddKnownProperty()
        {
            const string input = "{ \"unknown-property\": \"value\", \"known-property\": { \"unknown-sub-property1\": \"value1\", \"unknown-sub-property2\": \"value2\", \"unknown-sub-propertyN\": \"valueN\" } }";
            const string transformer = "{ \"#\": [ \"#copy($)\" ], \"known-property\": { \"additional-sub-property\": \"value\" } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.AddOrReplaceProperties }).Transform(transformer, input);

            Assert.AreEqual("{\"known-property\":{\"additional-sub-property\":\"value\",\"unknown-sub-property1\":\"value1\",\"unknown-sub-property2\":\"value2\",\"unknown-sub-propertyN\":\"valueN\"},\"unknown-property\":\"value\"}", result);
        }

        [Test]
        public void CopyReplaceProperty()
        {
            const string transformer = "{ \"#\": [ \"#copy($.menu)\" ], \"id\": 1 }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.AddOrReplaceProperties }).Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"id\":1,\"value\":{\"Window\":\"popup\"},\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":{\"action\":\"CreateNewDoc()\"}},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}],\"submenuitem\":\"CloseSession()\"}}", result);
        }

        [Test]
        public void CopyReplaceSubProperty()
        {
            const string transformer = "{ \"#\": [ \"#copy($.menu)\" ], \"popup\": { \"menuitem\": [] } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.AddOrReplaceProperties }).Transform(transformer, ExampleInputs.Menu);

            Assert.AreEqual("{\"popup\":{\"menuitem\":[],\"submenuitem\":\"CloseSession()\"},\"id\":{\"file\":\"csv\"},\"value\":{\"Window\":\"popup\"}}", result);
        }
    }
}
