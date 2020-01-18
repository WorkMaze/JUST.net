using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class ExistanceTests
    {
        [Test]
        public void Exists()
        {
            const string transformer = "{ \"IsBought\": \"#exists($.BuyDate)\", \"HasExpireDate\": \"#exists($.ExpireDate)\", \"HasDefects\": \"#exists($.Defects)\", \"HasNotes\": \"#exists($.Notes)\" }";
            const string input = "{ \"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"Defects\": \"\", \"Notes\": null }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":true,\"HasNotes\":true}", result);
        }

        [Test]
        public void ExistsNested()
        {
            const string transformer = "{ \"IsBought\": \"#exists(#valueof($.path))\", \"HasExpireDate\": \"#exists($.ExpireDate)\", \"HasDefects\": \"#exists($.Defects)\", \"HasNotes\": \"#exists($.Notes)\" }";
            const string input = "{ \"path\": \"$.BuyDate\",\"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"Defects\": \"\", \"Notes\": null }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":true,\"HasNotes\":true}", result);
        }

        [Test]
        public void ExistsAndNotEmpty()
        {
            const string transformer = "{ \"IsBought\": \"#existsandnotempty($.BuyDate)\", \"HasExpireDate\": \"#existsandnotempty($.ExpireDate)\", \"HasDefects\": \"#existsandnotempty($.Defects)\", \"HasNotes\": \"#existsandnotempty($.Notes)\" }";
            const string input = "{ \"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"Defects\": \"\", \"Notes\": null }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":false,\"HasNotes\":false}", result);
        }

        [Test]
        public void ExistsAndNotEmptyNested()
        {
            const string transformer = "{ \"IsBought\": \"#existsandnotempty(#valueof($.path))\", \"HasExpireDate\": \"#existsandnotempty($.ExpireDate)\", \"HasDefects\": \"#existsandnotempty($.Defects)\", \"HasNotes\": \"#existsandnotempty($.Notes)\" }";
            const string input = "{ \"path\": \"$.BuyDate\", \"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"Defects\": \"\", \"Notes\": null }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":false,\"HasNotes\":false}", result);
        }
    }
}
