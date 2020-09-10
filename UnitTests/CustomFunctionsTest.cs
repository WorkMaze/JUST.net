using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JUST.UnitTests
{
    [TestFixture, Category("CustomFunctions")]
    public class CustomFunctionsTests
    {
        [Test]
        public void XmlTest()
        {
            string inputSpecial = File.ReadAllText("Inputs/Input_Customfunction_Nestedresult.json");
            string transformer = File.ReadAllText("Inputs/Transformer_customfunctionnestedresult.json");
            string result = JsonConvert.SerializeObject
                (new JsonTransformer().Transform(JObject.Parse(transformer), JObject.Parse(inputSpecial)));
            Assert.AreEqual("{\"Seasons\":[[[[\"2017\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]],[[\"2018\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]],[[\"2019\"],[\"40\"],[\"20\"],[\"25\"],[\"10\"]]]]}", result);
        }
    }
}