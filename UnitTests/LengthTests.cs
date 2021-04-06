using JUST.net.Selectables;
using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture, Category("Length")]
    public class LengthTests
    {
        [Test]
        public void LengthString()
        {
            const string transformer = "{ \"length\": \"#length(somestring)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":10}", result);
        }

        [Test]
        public void LengthArray()
        {
            const string transformer = "{ \"length\": \"#length(#valueof($.numbers))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":5}", result);
        }

        [Test]
        public void LengthPath()
        {
            const string transformer = "{ \"length\": \"#length($.numbers)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":5}", result);
        }

        [Test]
        public void LengthJmesPath()
        {
            const string transformer = "{ \"length\": \"#length(numbers)\" }";

            var result = new JsonTransformer<JmesPathSelectable>().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":5}", result);
        }

        [Test]
        public void LengthJmesPathArray()
        {
            const string transformer = "{ \"length\": \"#length(#valueof(numbers))\" }";

            var result = new JsonTransformer<JmesPathSelectable>().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":5}", result);
        }

        [Test]
        public void LengthNotEnumerableValue()
        {
            const string transformer = "{ \"length\": \"#length(#valueof($.numbers[0]))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":0}", result);
        }

        [Test]
        public void LengthNotEnumerableConst()
        {
            const string transformer = "{ \"length\": \"#length(#tointeger(1))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"length\":0}", result);
        }

        [Test]
        public void LengthNotEnumerableValueStrict()
        {
            const string transformer = "{ \"length\": \"#length(#valueof($.numbers[0]))\" }";

            var result = Assert.Throws<Exception>(() => new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, ExampleInputs.NumbersArray));

            Assert.AreEqual("Error while calling function : #length(#valueof($.numbers[0])) - Argument not elegible for #length: 1", result.Message);
        }

        [Test]
        public void LengthNotEnumerableConstStrict()
        {
            const string transformer = "{ \"length\": \"#length(#todecimal(1.44))\" }";

            var result = Assert.Throws<Exception>(() => new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, ExampleInputs.NumbersArray));

            var decimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual($"Error while calling function : #length(#todecimal(1.44)) - Argument not elegible for #length: 1{decimalSeparator}44", result.Message);
        }
    }
}
