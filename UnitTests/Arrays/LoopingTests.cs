using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture, Category("Loops")]
    public class LoopingTests
    {
        [Test]
        public void CurrentValuePrimitive()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_value\": \"#currentvalue()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value\":1},{\"current_value\":2},{\"current_value\":3},{\"current_value\":4},{\"current_value\":5}]}", result);
        }

        [Test]
        public void CurrentValueObject()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value\": \"#currentvalue()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value\":{\"country\":{\"name\":\"Norway\",\"language\":\"norsk\"}}},{\"current_value\":{\"country\":{\"name\":\"UK\",\"language\":\"english\"}}},{\"current_value\":{\"country\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}}]}", result);
        }

        [Test]
        public void CurrentIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_index\": \"#currentindex()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_index\":0},{\"current_index\":1},{\"current_index\":2},{\"current_index\":3},{\"current_index\":4}]}", result);
        }

        [Test]
        public void LastIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_index\": \"#lastindex()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4}]}", result);
        }

        [Test]
        public void LastValuePrimitive()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_value\": \"#lastvalue()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5}]}", result);
        }

        [Test]
        public void LastValueObject()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value\": \"#lastvalue()\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value\":{\"country\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}},{\"last_value\":{\"country\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}},{\"last_value\":{\"country\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}}]}", result);
        }

        [Test]
        public void CurrentValueAtPathPrimitive()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value_at_path\":\"Norway\"},{\"current_value_at_path\":\"UK\"},{\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void CurrentValueAtPathObject()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value_at_path\": \"#currentvalueatpath($.country)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value_at_path\":{\"name\":\"Norway\",\"language\":\"norsk\"}},{\"current_value_at_path\":{\"name\":\"UK\",\"language\":\"english\"}},{\"current_value_at_path\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}]}", result);
        }

        [Test]
        public void LastValueAtPathPrimitive()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value_at_path\": \"#lastvalueatpath($.country.language)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"}]}", result);
        }

        [Test]
        public void LastValueAtPathObject()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value_at_path\": \"#lastvalueatpath($.country)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value_at_path\":{\"name\":\"Sweden\",\"language\":\"swedish\"}},{\"last_value_at_path\":{\"name\":\"Sweden\",\"language\":\"swedish\"}},{\"last_value_at_path\":{\"name\":\"Sweden\",\"language\":\"swedish\"}}]}", result);
        }

        [Test]
        public void NestedLooping()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loop($.Details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void ContextInputIsJToken()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void GlobalContextInputRestored()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } }, \"root\": \"#valueof($)\" }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}],\"root\":" + ExampleInputs.ObjectArray.Replace(" ", "") + "}", result);
        }

        [Test]
        public void NestedLoopingContextInput()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loop($.Details)\": { \"Exists\": \"#exists($.Country)\", \"IsIsland\": \"#ifcondition(#currentvalueatpath($.Country),Iceland,#toboolean(True),#toboolean(False))\", \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"Exists\":true,\"IsIsland\":true,\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"Exists\":true,\"IsIsland\":false,\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void FunctionAsLoopArgument()
        {
            const string transformer = "{ \"hello\": { \"#loop(#xconcat($.NestedLoop.,Organization,.Employee))\": { \"Details\": { \"#loop(#concat($.,Details))\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void PrimitiveTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalueatpath($.name)\" } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[\"Person 1\",\"Person 3\"]}", result);
        }

        [Test]
        public void ObjectTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalue()\" } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"id\":1,\"name\":\"Person 1\",\"gender\":\"M\"},{\"id\":3,\"name\":\"Person 3\",\"gender\":\"M\"}]}", result);
        }

        [Test]
        public void LoopOverProperties()
        {
            var input = "{ \"animals\": { \"cat\": { \"legs\": 4, \"sound\": \"meow\" }, \"dog\": { \"legs\": 4, \"sound\": \"woof\" } }, \"spell_numbers\": { \"3\": \"three\", \"2\": \"two\", \"1\": \"one\" } }";
            var transformer = "{ \"sounds\": { \"#loop($.animals)\": { \"#eval(#currentproperty())\": \"#currentvalueatpath($..sound)\" } }, \"number_index\": { \"#loop($.spell_numbers)\": { \"#eval(#currentindex())\": \"#currentvalueatpath(#concat($.,#currentproperty()))\" } }}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"sounds\":{\"cat\":\"meow\",\"dog\":\"woof\"},\"number_index\":{\"0\":\"three\",\"1\":\"two\",\"2\":\"one\"}}", result);
        }

        [Test]
        public void EmptyPropertiesLooping()
        {
            var input = "{ \"animals\": { } }";
            var transformer = "{ \"sounds\": { \"#loop($.animals)\": { \"#eval(#currentproperty())\": \"#currentvalueatpath($..sound)\" } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"sounds\":{}}", result);
        }

        [Test]
        public void NullLooping()
        {
            var input = "{ \"spell_numbers\": null }";
            var transformer = "{ \"number_index\": { \"#loop($.spell_numbers)\": { \"#eval(#currentindex())\": \"#currentvalueatpath(#concat($.,#currentproperty()))\" } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"number_index\":null}", result);
        }

        [Test]
        public void SingleResultFilter()
        {
            var input = "{\"array\":[{\"resource\":\"Location\",\"number\":\"3\" },{\"resource\":\"Organization\",\"number\":\"10\"}] }";
            var transformer = "{\"result\":{\"#loop($.array[?(@resource=='Location')])\":{\"existsLocation\":true}}}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"existsLocation\":true}]}", result);
        }

        [Test]
        public void SingleIndexReference()
        {
            var input = "{\"array\":[{\"resource\":\"Location\",\"number\":\"3\" },{\"resource\":\"Organization\",\"number\":\"10\"}] }";
            var transformer = "{\"result\": {\"#loop($.array[1])\": {\"number\":\"#currentvalueatpath($.number)\"} }}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"number\":\"10\"}]}", result);
        }

        [Test]
        public void LoopingAlias()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee, employee)\": { \"Details\": { \"#loop($.Details, details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country, details)\", \"OuterName\": \"#currentvalueatpath($.Name, employee)\", \"FirstLevel\": { \"#loop($.Roles, roles)\": { \"Employee\": \"#currentvalue(employee)\", \"Job\": \"#currentvalueatpath($.Job, roles)\" } } } } } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\",\"OuterName\":\"E2\",\"FirstLevel\":[{\"Employee\":{\"Name\":\"E2\",\"Details\":[{\"Country\":\"Iceland\",\"Age\":\"30\",\"Name\":\"Sven\",\"Language\":\"Icelandic\",\"Roles\":[{\"Job\":\"Janitor\",\"Salary\":100},{\"Job\":\"Security\",\"Salary\":200}]}]},\"Job\":\"Janitor\"},{\"Employee\":{\"Name\":\"E2\",\"Details\":[{\"Country\":\"Iceland\",\"Age\":\"30\",\"Name\":\"Sven\",\"Language\":\"Icelandic\",\"Roles\":[{\"Job\":\"Janitor\",\"Salary\":100},{\"Job\":\"Security\",\"Salary\":200}]}]},\"Job\":\"Security\"}]}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\",\"OuterName\":\"E1\",\"FirstLevel\":[{\"Employee\":{\"Name\":\"E1\",\"Details\":[{\"Country\":\"Denmark\",\"Age\":\"30\",\"Name\":\"Svein\",\"Language\":\"Danish\",\"Roles\":[{\"Job\":\"Manager\",\"Salary\":300},{\"Job\":\"Developer\",\"Salary\":400}]}]},\"Job\":\"Manager\"},{\"Employee\":{\"Name\":\"E1\",\"Details\":[{\"Country\":\"Denmark\",\"Age\":\"30\",\"Name\":\"Svein\",\"Language\":\"Danish\",\"Roles\":[{\"Job\":\"Manager\",\"Salary\":300},{\"Job\":\"Developer\",\"Salary\":400}]}]},\"Job\":\"Developer\"}]}]}]}", result);
        }

        [Test]
        public void MixedLoopingAlias()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee, employee)\": { \"Details\": { \"#loop($.Details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\", \"OuterName\": \"#currentvalueatpath($.Name, employee)\" } } } } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\",\"OuterName\":\"E2\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\",\"OuterName\":\"E1\"}]}]}", result);
        }

        [Test]
        public void BulkFunctions()
        {
            const string input = "{\"score_PCS\": [{\"data\": \"2020-04-08T10:20:21.335+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.5},{\"score_type\": \"pcs_help\",\"score_value\": 0.46},{\"score_type\": \"pcs_rum\",\"score_value\": 0.5},{\"score_type\": \"pcs_mag\",\"score_value\": 0.63}]},{\"data\": \"2020-04-09T10:22:03.267+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.38},{\"score_type\": \"pcs_help\",\"score_value\": 0.42},{\"score_type\": \"pcs_rum\",\"score_value\": 0.35},{\"score_type\": \"pcs_mag\",\"score_value\": 0.38}]},{\"data\": \"2020-04-09T10:23:05.748+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.44},{\"score_type\": \"pcs_help\",\"score_value\": 0.38},{\"score_type\": \"pcs_rum\",\"score_value\": 0.5},{\"score_type\": \"pcs_mag\",\"score_value\": 0.5}]}]}";
            const string transformer = "{ \"score_pcs_tot\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_tot')])\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_help\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_help')])\", \"#replace($.score_type, #currentvalueatpath($.score[?(@.score_type=='pcs_rum')]))\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_rum\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_rum')])\", \"#replace($.score_type, #currentvalueatpath($.score[?(@.score_type=='pcs_help')].score_type))\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_mag\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_mag')])\", \"#delete($.score_type)\" ], \"score_data\": \"#currentvalueatpath($.data)\" } } }";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"score_pcs_tot\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.5},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.38},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.44}],\"score_pcs_help\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.5},\"score_value\":0.46},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.35},\"score_value\":0.42},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.5},\"score_value\":0.38}],\"score_pcs_rum\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.5},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.35},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.5}],\"score_pcs_mag\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_value\":0.63},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_value\":0.38},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_value\":0.5}]}", result);
        }

        [Test]
        public void InsideLoopOverRoot()
        {
            const string input = "{ \"ontologyElements\": [ { \"id\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\", \"name\":\"Ontology1\",\"description\":\"test1\", \"entityType\":\"Ontology\" }, { \"id\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\", \"name\":\"MainType1\",\"order\":1, \"entityType\":\"MainType\", \"ontologyId\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\" }, { \"id\":\"97aa4eb2-0515-43d6-ba59-ffd931956b1a\", \"name\":\"SubType1\", \"order\":1, \"entityType\":\"SubType\", \"ontologyId\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\", \"mainTypeId\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\" } ] }";
            
            const string transformer = "{ \"id\": \"#valueof($.ontologyElements[?(@.entityType == 'Ontology')].id)\", \"description\": \"#valueof($.ontologyElements[?(@.entityType == 'Ontology')].description)\", \"maintypes\": { \"#loop($.ontologyElements[?(@.entityType == 'MainType')])\" : { \"id\": \"#currentvalueatpath($.id)\", \"name\": \"#currentvalueatpath($.name)\", \"order\": \"#currentvalueatpath($.order)\", \"subTypes\" : { \"#loop($.ontologyElements[?/(@.entityType == 'SubType' && @.ontologyId == '8b8b9d6e-4574-466b-b2c3-0062ad0642fe'/)], insideLoop, root)\": { \"name\": \"#currentvalueatpath($.name, insideLoop)\" } } } } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"id\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\",\"description\":\"test1\",\"maintypes\":[{\"id\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\",\"name\":\"MainType1\",\"order\":1,\"subTypes\":[{\"name\":\"SubType1\"}]}]}", result);
        }

        [Test]
        public void DynamicExpression()
        {
            const string input = "{ \"ontologyElements\": [ { \"id\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\", \"name\":\"Ontology1\",\"description\":\"test1\", \"entityType\":\"Ontology\" }, { \"id\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\", \"name\":\"MainType1\",\"order\":1, \"entityType\":\"MainType\", \"ontologyId\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\" }, { \"id\":\"97aa4eb2-0515-43d6-ba59-ffd931956b1a\", \"name\":\"SubType1\", \"order\":1, \"entityType\":\"SubType\", \"ontologyId\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\", \"mainTypeId\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\" } ] }";

            const string transformer = "{ \"id\": \"#valueof($.ontologyElements[?(@.entityType == 'Ontology')].id)\", \"description\": \"#valueof($.ontologyElements[?(@.entityType == 'Ontology')].description)\", \"maintypes\": { \"#loop($.ontologyElements[?(@.entityType == 'MainType')])\" : { \"id\": \"#currentvalueatpath($.id)\", \"name\": \"#currentvalueatpath($.name)\", \"order\": \"#currentvalueatpath($.order)\", \"subTypes\" : { \"#loop(#xconcat($.ontologyElements[?/(@.entityType == 'SubType' && @.ontologyId == ', #currentvalueatpath($.ontologyId),'/)]), #concat(inside,Loop), #concat(ro, ot))\": { \"name\": \"#currentvalueatpath($.name,insideLoop)\" } } } } }";

            var result = new JsonTransformer(new JUSTContext { EvaluationMode = EvaluationMode.Strict }).Transform(transformer, input);

            Assert.AreEqual("{\"id\":\"8b8b9d6e-4574-466b-b2c3-0062ad0642fe\",\"description\":\"test1\",\"maintypes\":[{\"id\":\"3ac89bbd-0de2-4692-a077-1d5d41efab69\",\"name\":\"MainType1\",\"order\":1,\"subTypes\":[{\"name\":\"SubType1\"}]}]}", result);
        }

        [Test]
        public void FunctionWithoutAlias()
        {
            const string transformer = "{ \"result\": { \"#loop($.NestedLoop.Organization.Employee, employee)\": { \"Name\": \"#currentvalueatpath($.Name)\"} } }";

            var result = new JsonTransformer().Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"result\":[{\"Name\":\"E2\"},{\"Name\":\"E1\"}]}", result);

        }

        [Test]
        public void TwoLoopsSingleProperty()
        {
            var input = "{ \"ComponentA\": [ { \"ComponentAId\": 1, \"ComponentAType\": \"T1\", \"ComponentAKind\": \"K1\" } ], \"ComponentB\": [ { \"ComponentBId\": 2, \"ComponentBType\": \"T2\", \"ComponentBKind\": \"K2\" } ]}";
            var transformer = "{ \"GenericComponent\": { \"#loop($.ComponentA)\": { \"GenericComponentId\": \"#currentvalueatpath($.ComponentAId)\", \"GenericComponentType\": \"#currentvalueatpath($.ComponentAType)\", \"GenericComponentKind\": \"#currentvalueatpath($.ComponentAKind)\" }, \"#loop($.ComponentB)\": { \"GenericComponentId\": \"#currentvalueatpath($.ComponentBId)\", \"GenericComponentType\": \"#currentvalueatpath($.ComponentBType)\", \"GenericComponentKind\": \"#currentvalueatpath($.ComponentBKind)\" } }}";

            var result = new JsonTransformer().Transform(transformer, input);

            Assert.AreEqual("{\"GenericComponent\":[{\"GenericComponentId\":1,\"GenericComponentType\":\"T1\",\"GenericComponentKind\":\"K1\"},{\"GenericComponentId\":2,\"GenericComponentType\":\"T2\",\"GenericComponentKind\":\"K2\"}]}", result);
        }

        [Test]
        public void LoopOverPropertiesInsideLoop()
        {
            var input = "{ \"Systems\": [ { \"Id\": \"SystemId1\", \"Name\": \"Name of system 1\", \"Components\": [ { \"Id\": \"CompId1\", \"Name\": \"comp 1\", \"Properties\": { \"PropA\": \"valueA\", \"Prop2\": 2.2 } }, { \"Id\": \"CompId2\", \"Name\": \"comp 2\", \"Properties\": { \"PropC\": \"valuec\", \"Prop2\": 222.222 } } ] }, { \"Id\": \"SystemId2\", \"Name\": \"Name of system 2\", \"Components\": [ { \"Id\": \"CompId3\", \"Name\": \"comp 3\", \"Properties\": { \"PropD\": \"valueD\" } }, { \"Id\": \"CompId4\", \"Name\": \"comp 4\", \"Properties\": { \"Prop1\": 11, \"Prop2\": 22.22 } } ] } ]}";
            var transformer = "{ \"result\": { \"#loop($.Systems,outer)\": { \"#eval(#xconcat(System.,#currentvalueatpath($.Id),.Name))\": \"#currentvalueatpath($.Name)\", \"components\": { \"#loop($.Components,inner)\": { \"#eval(#xconcat(System.,#currentvalueatpath($.Id,outer),.Componets.,#currentvalueatpath($.Id,inner)))\": \"#currentvalueatpath($.Name,inner)\", \"properties\": { \"#loop($.Properties)\": { \"#eval(#xconcat(System.,#currentvalueatpath($.Id,outer),.Components.,#currentvalueatpath($.Id,inner),.,#currentproperty()))\": \"#currentvalueatpath(#xconcat($.,#currentproperty()))\" } } } } } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = new JsonTransformer(context).Transform(transformer, input);

            Assert.AreEqual("{\"result\":[{\"components\":[{\"properties\":{\"System.SystemId1.Components.CompId1.PropA\":\"valueA\",\"System.SystemId1.Components.CompId1.Prop2\":2.2},\"System.SystemId1.Componets.CompId1\":\"comp 1\"},{\"properties\":{\"System.SystemId1.Components.CompId2.PropC\":\"valuec\",\"System.SystemId1.Components.CompId2.Prop2\":222.222},\"System.SystemId1.Componets.CompId2\":\"comp 2\"}],\"System.SystemId1.Name\":\"Name of system 1\"},{\"components\":[{\"properties\":{\"System.SystemId2.Components.CompId3.PropD\":\"valueD\"},\"System.SystemId2.Componets.CompId3\":\"comp 3\"},{\"properties\":{\"System.SystemId2.Components.CompId4.Prop1\":11,\"System.SystemId2.Components.CompId4.Prop2\":22.22},\"System.SystemId2.Componets.CompId4\":\"comp 4\"}],\"System.SystemId2.Name\":\"Name of system 2\"}]}", result);
        }
    }
}
