using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JUST.net.Selectables
{
    public class JsonPathSelectable : ISelectableToken
    {
        public string RootReference => "$.";
        public JToken Token { get; set; }

        public JToken Select(string path)
        {
            return Token.SelectToken(path);
        }

        public IEnumerable<JToken> SelectMultiple(string path)
        {
            return Token.SelectTokens(path);
        }
    }
}
