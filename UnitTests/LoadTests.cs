using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace JUST.UnitTests
{
    [TestFixture]
    public class LoadTests
    {
        [Test]
        public void LargeInput()
        {
            var input = File.ReadAllText("Inputs/large_input.json");
            const string transformer = "{ \"result\": { \"#loop($.list)\": { \"id\": \"#currentindex()\", \"name\": \"#concat(#currentvalueatpath($.title), #currentvalueatpath($.name))\", \"contact\": \"#currentvalueatpath($.contacts[?(@.is_default==true)])\", \"address\": \"#currentvalueatpath($.addresses[0])\" } }";

            var w = Stopwatch.StartNew();
            new JsonTransformer().Transform(transformer, input);
            w.Stop();
            var timeConsumed = w.Elapsed;
            Assert.LessOrEqual(timeConsumed, TimeSpan.FromSeconds(4));
        }

        [Test]
        public void LargeTransformer()
        {
            const string input = "{ \" title\" : \" Mr.\" , \" name\" : \" Smith\" , \" addresses\" : [ { \" street\" : \" Some Street\" , \" number\" : 1, \" city\" : \" Some City\" , \" postal_code\" : 1234 }, { \" street\" : \" Some Other Street\" , \" number\" : 2, \" city\" : \" Some Other City\" , \" postal_code\" : 5678 } ], \" contacts\" : [ { \" type\" : \" home\" , \" number\" : 123546789, \" is_default\" : false }, { \" type\" : \" mobile\" , \" number\" : 987654321, \" is_default\" : true } ] }";
            var transformer = File.ReadAllText("Inputs/large_transformer.json");

            var w = Stopwatch.StartNew();
            new JsonTransformer().Transform(transformer, input);
            w.Stop();
            var timeConsumed = w.Elapsed;
            Assert.LessOrEqual(timeConsumed, TimeSpan.FromSeconds(3));
        }
    }
}
