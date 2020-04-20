using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture]
    public class ArgumentsEscapeTests
    {
        private const string EscapeChar = "/";
        [Test]
        public void NoEscapedCharacters()
        {
            const string args = "arg1";
            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\"arg1#\"}", result);
        }

        [Test]
        public void EscapedBrackets()
        {
            var args = $"{EscapeChar}({EscapeChar})";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\"()#\"}", result);
        }

        [Test]
        public void EscapedSingleQuotes()
        {
            var args = $"{EscapeChar}'{EscapeChar}'";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\"''#\"}", result);
        }

        [Test]
        public void EscapedComma()
        {
            var args = $"{EscapeChar},";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, "{}");

            Assert.AreEqual("{\"result\":\",#\"}", result);
        }

        [Test]
        public void EscapedSharp()
        {
            var args = $"{EscapeChar}#";
            var transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, "{}");

            Assert.AreEqual($"{{\"result\":\"##\"}}", result);
        }

        [Test]
        public void QuotedArgument()
        {
            const string args = "'arg1,#func(arg2.1,arg2.2)'";
            const string input = "{ \"test\": \"" + args + "\" }";

            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"arg1,#func(arg2.1,arg2.2)#\"}", result);
        }

        [Test]
        public void NestedFunctionEscapedArguments()
        {
            const string args = "#arg1,#xconcat(/#notfunc/(/), #constant_comma(),#xconcat(/,arg2.3.1,#constant_hash(),/'arg2.3.3),/,,#add(3,2))";
            const string input = "{ \"test\": \"" + args + "\" }";

            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"#arg1#notfunc(),,arg2.3.1#'arg2.3.3,5#\"}", result);
        }

        [Test]
        public void MixedArguments()
        {
            const string args = "arg1" +
                ",#xconcat(arg2.1,#constant_comma(),#xconcat(arg2.3.1,#constant_hash(),arg2.3.3),#add(1,2))" +
                ",'arg3.1,arg3.2,func(arg3.3.1,arg3.3.2)'" +
                ",arg4/(.1/)" +
                ",arg5/,";
            const string input = "{ \"test\": \"" + args + "\" }";

            const string transformer = "{ \"result\": \"#xconcat(" + args + ",#constant_hash())\" }";
            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"arg1arg2.1,arg2.3.1#arg2.3.33arg3.1,arg3.2,func(arg3.3.1,arg3.3.2)arg4(.1)arg5,#\"}", result);
        }

        [Test]
        public void ConsecutiveEscapedCharacters()
        {
            const string transformer = "{ \"result\": \"#xconcat(///),_end)\" }";
            var result = JsonTransformer.Transform(transformer, "{}");
            Assert.AreEqual("{\"result\":\"/)_end\"}",result);
        }

        [Test]
        public void TestIssue59()
        {
            const string input = "{\"PhoneTypeToSearch\": \"iPhone\",\"Address\": [{\"City\": \"NewYork\",\"name\": \"Jim\"},{\"City\": \"NewYork\",\"name\": \"John\"}],\"people\": [{\"name\": \"Jim\",\"phoneNumbers\": [{\"type\": \"iPhone\",\"number\": \"0123-4567-8888\",\"countryPrefix\": \"34\"},{\"type\": \"work\",\"number\": \"012567-8910\"}]},{\"name\": \"John\",\"phoneNumbers\": [{\"type\": \"iPhone\",\"number\": \"0123-4562347-8888\",\"countryPrefix\": \"43\"},{\"type\": \"home\",\"number\": \"0134523-4567-8910\"}]},{\"name\": \"John\"}]}";

            const string transformer = "{ \"Persons\": { \"#loop($.people)\": { \"Address\": \"#valueof(#xconcat($.Address[?,/(@.name==',#currentvalueatpath($.name),/'/)]))\" } }}";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"Persons\":[{\"Address\":{\"City\":\"NewYork\",\"name\":\"Jim\"}},{\"Address\":{\"City\":\"NewYork\",\"name\":\"John\"}},{\"Address\":{\"City\":\"NewYork\",\"name\":\"John\"}}]}", result);
        }
    }
}
