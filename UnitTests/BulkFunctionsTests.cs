using NUnit.Framework;

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
        public void Replace()
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
        public void ReadmeTest()
        {
            const string transformer = "{ \"#\": [ \"#copy($)\", \"#delete($.tree.branch.bird)\", \"#replace($.tree.branch.extra,#valueof($.tree.ladder))\" ], \"othervalue\" : \"othervalue\" }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.Tree);

            Assert.AreEqual("{\"othervalue\":\"othervalue\",\"tree\":{\"branch\":{\"leaf\":\"green\",\"flower\":\"red\",\"extra\":{\"wood\":\"treehouse\"}},\"ladder\":{\"wood\":\"treehouse\"}}}", result);
        }
    }
}
