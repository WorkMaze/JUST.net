using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace JUST.net.Selectables
{
    public class JsonPathSelectable : ISelectableToken
    {
        public string RootReference => "$.";
        public JToken Token { get; set; }

        public JToken Select(string path)
        {
            try
            {
                return Token.SelectToken(path);
            }
            catch (JsonException)
            {
                var result = Token.SelectTokens(path);
                return new JArray(result.ToArray());
            }
        }
    }
}
