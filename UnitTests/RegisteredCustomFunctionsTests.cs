using NUnit.Framework;
using System;

namespace JUST.UnitTests
{
    [TestFixture, Category("CustomFunctions")]
    public class RegisteredCustomFunctionsTests
    {
        private JUSTContext _context;
        [SetUp]
        public void Setup()
        {
            _context = new JUSTContext();
        }

        [Test]
        public void SetupTest()
        {
            Assert.Pass();
        }

        [Test]
        public void ExternalStaticMethod()
        {
            const string input = "{ }";
            const string transformer = "{ \"result\": \"#StaticMethod()\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "StaticMethod");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"External Static\"}", result);
        }

        [Test]
        public void ExternalStaticTypedParameters()
        {
            const string input = "{ }";
            const string transformer = "{ \"result\": \"#StaticTypedParameters(1,true,abc,2018-10-11T11:00:00.000Z)\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "StaticTypedParameters");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"External Static TypedParameters success\"}", result);
        }

        [Test]
        public void ExternalInstanceMethod()
        {
            const string input = "{ }";
            const string transformer = "{ \"result\": \"#InstanceMethod()\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "InstanceMethod");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"External Instance\"}", result);
        }

        [Test]
        public void ExternalTypedParameters()
        {
            const string input = "{ }";
            const string transformer = "{ \"result\": \"#TypedParameters(1,true,abc,2018-10-11T11:00:00.000Z)\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "TypedParameters");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"External TypedParameters success\"}", result);
        }

        [Test]
        public void ExternalNavigateTypedParameters()
        {
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"result\": \"#NavigateTypedParameters(#valueof($.lvl1.some-bool))\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "NavigateTypedParameters");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":\"True\"}", result);
        }

        [Test]
        public void ExternalNavigateTypedNullParameters()
        {
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"result\": \"#NavigateTypedNullParameters(#valueof($.non-existent))\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "NavigateTypedNullParameters");
            var result = new JsonTransformer(_context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":null}", result);
        }

        [Test]
        public void InvalidFunctionInTransformer()
        {
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"result\": \"#SomeInvalid(#valueof($.non-existent))\" }";

            _context.RegisterCustomFunction("ExternalMethods", "ExternalMethods.ExternalClass", "NavigateTypedNullParameters");
            var result = Assert.Throws<Exception>(() => new JsonTransformer(_context).Transform(transformer, input));

            Assert.AreEqual("Invalid function: #SomeInvalid", result.InnerException.Message);
        }

        [Test]
        public void RegisterInContextConstructor()
        {
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"result\": \"#NavigateTypedNullParameters(#valueof($.non-existent))\" }";

            var context = new JUSTContext(new[] {
                new CustomFunction {
                    AssemblyName = "ExternalMethods",
                    Namespace = "ExternalMethods.ExternalClass",
                    MethodName = "NavigateTypedNullParameters"
                }
            });
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":null}", result);
        }

        [Test]
        public void UnregisterCustomFunction()
        {
            const string unregisteredFunction = "NavigateTypedNullParameters";
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"instance\": \"#InstanceMethod()\", \"navigated\": \"#NavigateTypedNullParameters(#valueof($.non-existent))\" }";

            var context = new JUSTContext(new[] {
                new CustomFunction {
                    AssemblyName = "ExternalMethods",
                    Namespace = "ExternalMethods.ExternalClass",
                    MethodName = "InstanceMethod"
                },
                new CustomFunction {
                    AssemblyName = "ExternalMethods",
                    Namespace = "ExternalMethods.ExternalClass",
                    MethodName = unregisteredFunction
                }
            });

            context.UnregisterCustomFunction(unregisteredFunction);
            
            var result = Assert.Throws<Exception>(() => new JsonTransformer(context).Transform(transformer, input));
            Assert.AreEqual($"Error while calling function : #{unregisteredFunction}(#valueof($.non-existent)) - Invalid function: #{unregisteredFunction}", result.Message);
        }

        [Test]
        public void ClearCustomFunctions()
        {
            const string unregisteredFunction = "NavigateTypedNullParameters";
            const string input = "{ \"lvl1\": { \"some-bool\": true } }";
            const string transformer = "{ \"navigated\": \"#NavigateTypedNullParameters(#valueof($.non-existent))\" }";

            var context = new JUSTContext(new[] {
                new CustomFunction {
                    AssemblyName = "ExternalMethods",
                    Namespace = "ExternalMethods.ExternalClass",
                    MethodName = unregisteredFunction
                }
            });

            context.ClearCustomFunctionRegistrations();

            var result = Assert.Throws<Exception>(() => new JsonTransformer(context).Transform(transformer, input));
            Assert.AreEqual($"Error while calling function : #{unregisteredFunction}(#valueof($.non-existent)) - Invalid function: #{unregisteredFunction}", result.Message);
        }
    }
}
