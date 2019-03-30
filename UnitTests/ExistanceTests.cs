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

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":true,\"HasNotes\":true}", result);
        }

        [Test]
        public void ExistsAndNotEmpty()
        {
            const string transformer = "{ \"IsBought\": \"#existsandnotempty($.BuyDate)\", \"HasExpireDate\": \"#existsandnotempty($.ExpireDate)\", \"HasDefects\": \"#existsandnotempty($.Defects)\", \"HasNotes\": \"#existsandnotempty($.Notes)\" }";
            const string input = "{ \"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"Defects\": \"\", \"Notes\": null }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"IsBought\":true,\"HasExpireDate\":false,\"HasDefects\":false,\"HasNotes\":false}", result);
        }

        [Test]
        public void ReadmeTest()
        {
            const string transformer = "{ \"BuyDateString\": \"#ifcondition(#exists($.BuyDate),true,#concat(Buy Date : ,#valueof($.BuyDate)),NotExists)\", \"BuyDateString2\": \"#ifcondition(#existsandnotempty($.BuyDate),true,#concat(Buy Date : ,#valueof($.BuyDate)),EmptyOrNotExists)\", \"ExpireDateString\": \"#ifcondition(#exists($.ExpireDate),true,#concat(Expire Date : ,#valueof($.ExpireDate)),NotExists)\", \"ExpireDateString2\": \"#ifcondition(#existsandnotempty($.ExpireDate),true,#concat(Expire Date : ,#valueof($.ExpireDate)),EmptyOrNotExists)\", \"SellDateString\": \"#ifcondition(#exists($.SellDate),true,#concat(Sell Date : ,#valueof($.SellDate)),NotExists)\", \"SellDateString2\": \"#ifcondition(#existsandnotempty($.SellDate),true,#concat(Sell Date : ,#valueof($.SellDate)),EmptyOrNotExists)\" }";
            const string input = "{ \"BuyDate\": \"2017-04-10T11:36:39+03:00\", \"ExpireDate\": \"\" }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"BuyDateString\":\"Buy Date : 2017-04-10T11:36:39+03:00\",\"BuyDateString2\":\"Buy Date : 2017-04-10T11:36:39+03:00\",\"ExpireDateString\":\"Expire Date : \",\"ExpireDateString2\":\"EmptyOrNotExists\",\"SellDateString\":\"NotExists\",\"SellDateString2\":\"EmptyOrNotExists\"}", result);
        }
    }
}
