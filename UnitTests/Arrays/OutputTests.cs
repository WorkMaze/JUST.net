using NUnit.Framework;

namespace JUST.UnitTests.Issues
{
    [TestFixture, Category("Output")]
    public class OutputTests
    {
        [Test]
        public void OutputAsArray()
        {
            var input = "[{\"id\": 1, \"cnt\": 100, \"rowNum\": 1, \"col\": 1},{\"id\": 2, \"cnt\": 89, \"rowNum\": 1, \"col\": 1 }]";
            var transformer = "[{ \"#loop($)\": { \"key\": \"#currentvalueatpath($.id)\", \"quantity\": \"#currentvalueatpath($.cnt)\" } }]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict | EvaluationMode.JoinArrays
            };
            var result = new JsonTransformer(context).Transform(transformer, input);
            Assert.AreEqual("[{\"key\":1,\"quantity\":100},{\"key\":2,\"quantity\":89}]", result);
        }

        [Test]
        public void ArrayAsLoopResult()
        {
            var input = "[{\"id\": 1, \"cnt\": 100, \"rowNum\": 1, \"col\": 1},{\"id\": 2, \"cnt\": 89, \"rowNum\": 1, \"col\": 1 }]";
            var transformer = "{ \"#loop($)\": { \"key\": \"#currentvalueatpath($.id)\", \"quantity\": \"#currentvalueatpath($.cnt)\" } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict | EvaluationMode.JoinArrays
            };
            var result = new JsonTransformer(context).Transform(transformer, input);
            Assert.AreEqual("[{\"key\":1,\"quantity\":100},{\"key\":2,\"quantity\":89}]", result);
        }

        [Test]
        public void ArrayAsValueOfResult()
        {
            var input = "[{\"id\": 1, \"cnt\": 100, \"rowNum\": 1, \"col\": 1},{\"id\": 2, \"cnt\": 89, \"rowNum\": 1, \"col\": 1 }]";
            var transformer = "[{ \"a\": \"#valueof($)\" }, { \"#loop($)\": { \"key\": \"#currentvalueatpath($.id)\", \"quantity\": \"#currentvalueatpath($.cnt)\" } }]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict | EvaluationMode.JoinArrays
            };
            var result = new JsonTransformer(context).Transform(transformer, input);
            Assert.AreEqual("[{\"a\":[{\"id\":1,\"cnt\":100,\"rowNum\":1,\"col\":1},{\"id\":2,\"cnt\":89,\"rowNum\":1,\"col\":1}]},{\"key\":1,\"quantity\":100},{\"key\":2,\"quantity\":89}]", result);
        }

        [Test]
        public void ArrayWithArrayAsMember()
        {
            var input = "[{\"id\": 1, \"cnt\": 100, \"rowNum\": 1, \"col\": 1},{\"id\": 2, \"cnt\": 89, \"rowNum\": 1, \"col\": 1 }]";
            var transformer = "[{ \"a\": \"#valueof($)\" }, { \"#loop($)\": { \"key\": \"#currentvalueatpath($.id)\", \"quantity\": \"#currentvalueatpath($.cnt)\" } }]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);
            Assert.AreEqual("[{\"a\":[{\"id\":1,\"cnt\":100,\"rowNum\":1,\"col\":1},{\"id\":2,\"cnt\":89,\"rowNum\":1,\"col\":1}]},[{\"key\":1,\"quantity\":100},{\"key\":2,\"quantity\":89}]]", result);
        }

        [Test]
        public void JoinArrays()
        {
            var input = "{\"Order\": {\"OrderId\": 123456,\"OrderLines\": [{\"SkuId\": 357159,\"Quantity\": 12.5},{\"SkuId\": 484186,\"Quantity\": 10}]}}";
            var transformer = "[{ \"#loop($.Order.OrderLines)\": { \"OrderId\": \"#valueof($.Order.OrderId)\", \"SkuId\": \"#currentvalueatpath($.SkuId)\", \"Quantity\": \"#currentvalueatpath($.Quantity)\" } }]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict | EvaluationMode.JoinArrays
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("[{\"OrderId\":123456,\"SkuId\":357159,\"Quantity\":12.5},{\"OrderId\":123456,\"SkuId\":484186,\"Quantity\":10}]", result);
        }

        [Test]
        public void ArrayWithArrayInside()
        {
            var input = "{\"Order\": {\"OrderId\": 123456,\"OrderLines\": [{\"SkuId\": 357159,\"Quantity\": 12.5},{\"SkuId\": 484186,\"Quantity\": 10}]}}";
            var transformer = "[{ \"#loop($.Order.OrderLines)\": { \"OrderId\": \"#valueof($.Order.OrderId)\", \"SkuId\": \"#currentvalueatpath($.SkuId)\", \"Quantity\": \"#currentvalueatpath($.Quantity)\" } }]";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("[[{\"OrderId\":123456,\"SkuId\":357159,\"Quantity\":12.5},{\"OrderId\":123456,\"SkuId\":484186,\"Quantity\":10}]]", result);
        }
    }
}