using NUnit.Framework;
using System;

namespace JUST.UnitTests.Arrays
{
    [TestFixture]
    public class AggregateFunctionsTests
    {
        [Test]
        public void ConcatAll()
        {
            const string transformer = "{ \"concat_all\": \"#concatall(#valueof($.d))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.StringsArray);

            Assert.AreEqual("{\"concat_all\":\"onetwothree\"}", result);
        }

        [Test]
        public void ConcatAllStrictError()
        {
            const string input = "{ \"arr\": [ \"string\", 0 ] }";
            const string transformer = "{ \"concat_all\": \"#concatall(#valueof($.arr))\" }";

            var result = Assert.Throws<Exception>(() => new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input));

            Assert.AreEqual("Error while calling function : #concatall(#valueof($.arr)) - Invalid value in array to concatenate: 0", result.Message);
        }

        [Test]
        public void ConcatAllAtPath()
        {
            const string transformer = "{ \"concat_all_at_path\": \"#concatallatpath(#valueof($.x),$.v.a)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultiDimensionalArray);

            Assert.AreEqual("{\"concat_all_at_path\":\"a1,a2,a3b1,b2c1,c2,c3\"}", result);
        }

        [Test]
        public void ConcatAllAtPathStrictError()
        {
            const string input = "{ \"arr\": [ { \"str\": \"\" }, { \"str\": 0 }] }";
            const string transformer = "{ \"concat_all_at_path\": \"#concatallatpath(#valueof($.arr),$.str)\" }";

            var result = Assert.Throws<Exception>(() => new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input));

            Assert.AreEqual("Error while calling function : #concatallatpath(#valueof($.arr),$.str) - Invalid value in array to concatenate: 0", result.Message);
        }

        [Test]
        public void Sum()
        {
            const string transformer = "{ \"sum\": \"#sum(#valueof($.numbers))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"sum\":15}", result);
        }

        [Test]
        public void SumAtPath()
        {
            const string transformer = "{ \"sum_at_path\": \"#sumatpath(#valueof($.x),$.v.c)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultiDimensionalArray);

            Assert.AreEqual("{\"sum_at_path\":60}", result);
        }

        [Test]
        public void Average()
        {
            const string transformer = "{ \"avg\": \"#average(#valueof($.numbers))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"avg\":3}", result);
        }

        [Test]
        public void AverageAtPath()
        {
            const string transformer = "{ \"avg_at_path\": \"#averageatpath(#valueof($.x),$.v.c)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultiDimensionalArray);

            Assert.AreEqual("{\"avg_at_path\":20}", result);
        }

        [Test]
        public void Min()
        {
            const string transformer = "{ \"min\": \"#min(#valueof($.numbers))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"min\":1}", result);
        }

        [Test]
        public void MinAtPath()
        {
            const string transformer = "{ \"min_at_path\": \"#minatpath(#valueof($.x),$.v.b)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultiDimensionalArray);

            Assert.AreEqual("{\"min_at_path\":1}", result);
        }

        [Test]
        public void Max()
        {
            const string transformer = "{ \"max\": \"#max(#valueof($.numbers))\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"max\":5}", result);
        }

        [Test]
        public void MaxAtPath()
        {
            const string transformer = "{ \"max_at_path\": \"#maxatpath(#valueof($.x),$.v.b)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.MultiDimensionalArray);

            Assert.AreEqual("{\"max_at_path\":3}", result);
        }
    }
}
