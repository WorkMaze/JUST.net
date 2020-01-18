using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class XFunctionsTests
    {
        [Test]
        public void Xconcat()
        {
            const string transformer = "{ \"FullName\": \"#xconcat(#valueof($.Name),#constant_comma(),#valueof($.MiddleName),#constant_comma(),#valueof($.Surname))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultipleArgs);

            Assert.AreEqual("{\"FullName\":\"Kari,Inger,Nordmann\"}", result);
        }

        [Test]
        public void Xadd()
        {
            const string transformer = "{ \"AgeOfParents\": \"#xadd(#valueof($.AgeOfMother),#valueof($.AgeOfFather))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultipleArgs);

            Assert.AreEqual("{\"AgeOfParents\":137}", result);
        }
    }
}
