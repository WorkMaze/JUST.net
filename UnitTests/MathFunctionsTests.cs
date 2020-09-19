using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class MathFunctionsTests
    {
        [Test]
        public void Add()
        {
            var transformer = "{ \"mathresult\": { \"add\": \"#add(#valueof($.numbers[0]),3)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"add\":4}}", result);
        }

        [Test]
        public void Subtract()
        {
            var transformer = "{ \"mathresult\": { \"subtract\": \"#subtract(#valueof($.numbers[4]),#valueof($.numbers[0]))\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"subtract\":4}}", result);
        }

        [Test]
        public void Multiply()
        {
            var transformer = "{ \"mathresult\": { \"multiply\": \"#multiply(2,#valueof($.numbers[2]))\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"multiply\":6}}", result);
        }

        [Test]
        public void Divide()
        {
            var transformer = "{ \"mathresult\": { \"divide\": \"#divide(9,#valueof($.numbers[2]))\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"divide\":3}}", result);
        }

        [TestCase("0.00154", "0.00", 2)]
        [TestCase("0.01554", "0.02", 2)]
        [TestCase("0.66489", "1.0", 0)]
        public void Round(string typedValue, string expectedResult, int decimalPlaces)
        {
            var input = $"{{ \"value\": {typedValue} }}";
            var transformer = $"{{ \"result\": \"#round(#valueof($.value),{decimalPlaces})\" }}";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual($"{{\"result\":{expectedResult}}}", result);
        }

        [Test]
        public void Equals()
        {
            var transformer = "{ \"mathresult\": { \"mathequals\": \"#mathequals(#valueof($.numbers[2]),3)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"mathequals\":true}}", result);
        }

        [Test]
        public void GreaterThan()
        {
            var transformer = "{ \"mathresult\": { \"mathgreaterthan\": \"#mathgreaterthan(#valueof($.numbers[2]),2)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"mathgreaterthan\":true}}", result);
        }

        [Test]
        public void LessThan()
        {
            var transformer = "{ \"mathresult\": { \"mathlessthan\": \"#mathlessthan(#valueof($.numbers[2]),4)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"mathlessthan\":true}}", result);
        }

        [Test]
        public void GreaterThanOrEqualsTo()
        {
            var transformer = "{ \"mathresult\": { \"mathgreaterthanorequalto\": \"#mathgreaterthanorequalto(#valueof($.numbers[2]),4)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"mathgreaterthanorequalto\":false}}", result);
        }

        [Test]
        public void LessThanOrEqualsTo()
        {
            var transformer = "{ \"mathresult\": { \"mathlessthanorequalto\": \"#mathlessthanorequalto(#valueof($.numbers[2]),2)\" }}";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"mathresult\":{\"mathlessthanorequalto\":false}}", result);
        }
    }
}
