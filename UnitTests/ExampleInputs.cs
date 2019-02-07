namespace JUST.UnitTests
{
    public class ExampleInputs
    {
        internal const string Menu = "{ \"menu\": { \"id\": { \"file\": \"csv\" }, \"value\": { \"Window\": \"popup\" }, \"popup\": { \"menuitem\": [ { \"value\": \"New\", \"onclick\": { \"action\": \"CreateNewDoc()\" } }, { \"value\": \"Open\", \"onclick\": \"OpenDoc()\" }, { \"value\": \"Close\", \"onclick\": \"CloseDoc()\" } ] } } }";
        internal const string ArrayX = "{ \"x\": [ { \"v\": { \"a\": \"a1,a2,a3\", \"b\": \"1\", \"c\": \"10\" } }, { \"v\": { \"a\": \"b1,b2\", \"b\": \"2\", \"c\": \"20\" } }, { \"v\": { \"a\": \"c1,c2,c3\", \"b\": \"3\", \"c\": \"30\" } } ]}";
        internal const string StringRef = "{\"stringref\": \"thisisandveryunuasualandlongstring\"}";
        internal const string NumbersArray = "{\"numbers\": [ \"1\", \"2\", \"3\", \"4\", \"5\" ]}";
        internal const string StringsArray = "{\"d\": [ \"one\", \"two\", \"three\" ]}";
        internal const string ObjectArray = "{ \"arrayobjects\": [ {\"country\": {\"name\": \"Norway\",\"language\": \"norsk\"}}, { \"country\": { \"name\": \"UK\", \"language\": \"english\" } }, { \"country\": { \"name\": \"Sweden\", \"language\": \"swedish\" } }] }";
        internal const string MultiDimensionalArray = "{\"x\": [ { \"v\": { \"a\": \"a1,a2,a3\", \"b\": \"1\", \"c\": \"10\" } }, { \"v\": { \"a\": \"b1,b2\", \"b\": \"2\", \"c\": \"20\" } }, { \"v\": { \"a\": \"c1,c2,c3\", \"b\": \"3\", \"c\": \"30\" } } ]}";
        internal const string Tree = "{ \"tree\": { \"branch\": { \"leaf\": \"green\", \"flower\": \"red\", \"bird\": \"crow\", \"extra\": { \"twig\":\"birdnest\" } }, \"ladder\": {\"wood\": \"treehouse\" } } }";
        internal const string NestedArrays = "{ \"NestedLoop\": { \"Organization\": { \"Employee\": [ { \"Name\": \"E2\", \"Details\": [ { \"Country\": \"Iceland\", \"Age\": \"30\", \"Name\": \"Sven\", \"Language\": \"Icelandic\" } ] }, { \"Name\": \"E1\", \"Details\": [ { \"Country\": \"Denmark\", \"Age\": \"30\", \"Name\": \"Svein\", \"Language\": \"Danish\" } ] } ] } } }";
        internal const string MultipleArgs = "{ \"Name\": \"Kari\", \"Surname\": \"Nordmann\", \"MiddleName\": \"Inger\", \"ContactInformation\": \"Karl johans gate:Oslo:88880000\" , \"PersonalInformation\": \"45:Married:Norwegian\", \"AgeOfMother\": 67, \"AgeOfFather\": 70 }";
        
    }
}
