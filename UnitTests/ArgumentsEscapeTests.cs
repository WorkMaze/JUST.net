using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("EscapedChars")]
    public class ArgumentsEscapeTests
    {
        [Test]
        public void NoEscapedCharacters()
        {
            const string args = "arg1";
            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\"arg1#\"}", result);
        }

        [Test]
        public void EscapedBrackets()
        {
            const char escapeChar = '/';
            var args = $"{escapeChar}({escapeChar})";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\"()#\"}", result);
        }

        [Test]
        public void EscapedComma()
        {
            const char escapeChar = '/';
            var args = $"{escapeChar},";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\",#\"}", result);
        }

        [Test]
        public void EscapedSharpValue()
        {
            const char escapeChar = '/';
            var args = $"{escapeChar}#";
            var transformer = "{ \"result\": \"" + args + "\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");

            Assert.AreEqual($"{{\"result\":\"#\"}}", result);
        }

        [Test]
        public void EscapedSharpArgument()
        {
            const char escapeChar = '/';
            var args = $"{escapeChar}#";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");

            Assert.AreEqual($"{{\"result\":\"##\"}}", result);
        }

        [Test]
        public void NestedFunctionEscapedArguments()
        {
            const char escapeChar = '/';
            string args = $"#arg1,#xconcat({escapeChar}#notfunc{escapeChar}({escapeChar}), #constant_comma(),#xconcat({escapeChar},arg2.3.1,#constant_hash(),'arg2.3.3),{escapeChar},,#add(3,2))";
            string input = "{ \"test\": \"" + args + "\" }";
            string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            
            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"#arg1#notfunc(),,arg2.3.1#'arg2.3.3,5#\"}", result);
        }

        [Test]
        public void MixedArguments()
        {
            const string args = "arg1" +
                ",#xconcat(arg2.1,#constant_comma(),#xconcat(arg2.3.1,#constant_hash(),arg2.3.3),#add(1,2))" +
                ",arg4/(.1/)" +
                ",arg5/,";
            const string input = "{ \"test\": \"" + args + "\" }";

            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"arg1arg2.1,arg2.3.1#arg2.3.33arg4(.1)arg5,#\"}", result);
        }

        [Test]
        public void ConsecutiveEscapedCharacters()
        {
            const string transformer = "{ \"result\": \"#xconcat(///),_end)\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");
            Assert.AreEqual("{\"result\":\"/)_end\"}", result);
        }

        [Test]
        public void EscapedEscapeCharacter()
        {
            const string transformer = "{ \"result\": \"#xconcat(//,_end)\" }";
            var result = new JsonTransformer().Transform(transformer, "{}");
            Assert.AreEqual("{\"result\":\"/_end\"}", result);
        }

        [Test]
        public void OneArgumentFunctionWithEscapedChars()
        {
            const string input = "{ \"creditsDebits\": [{ \"ratingModifierValue\": null, \"name\": \"Qualifications\", \"id\": \"3d4f2273-edb3-4959-b232-e7386e8dca1e\", \"factorValue\": null, \"factorMin\": null, \"factorMax\": null, \"defaultValue\": null, \"code\": \"ECC002\" }, { \"ratingModifierValue\": \"Need this value\", \"name\": \"Loss Experience\", \"id\": \"af0324f3-6676-4faf-a9e9-c14f1eaa2fee\", \"factorValue\": null, \"factorMin\": null, \"factorMax\": null, \"defaultValue\": null, \"code\": \"ECC100\" }] }";
            string transformer = "{ \"someNewNode\": \"#valueof($.creditsDebits[?/(@.code == 'ECC100'/)].ratingModifierValue)\" }";

            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"someNewNode\":\"Need this value\"}", result);
        }

        [Test]
        public void OtherEscapeChar()
        {
            const char escapeChar = '§';
            string args = $"#arg1,#xconcat({escapeChar}#notfunc{escapeChar}({escapeChar}), #constant_comma(),#xconcat({escapeChar},arg2.3.1,#constant_hash(),'arg2.3.3),{escapeChar},,#add(3,2))";
            string input = "{ \"test\": \"" + args + "\" }";
            string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var context = new JUSTContext { EscapeChar = '§' };

            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"#arg1#notfunc(),,arg2.3.1#'arg2.3.3,5#\"}", result);
        }
    }
}
