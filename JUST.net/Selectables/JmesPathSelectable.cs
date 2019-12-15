using DevLab.JmesPath;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JUST.net.Selectables
{
    public class JmesPathSelectable : ISelectableToken
    {
        private readonly JmesPath _instance = new JmesPath();

        public string RootReference => string.Empty;
        public JToken Token { get; set; }

        public JToken Select(string path)
        {
            return _instance.Transform(Token, path);
        }

        public IEnumerable<JToken> SelectMultiple(string path)
        {
            return Select(path);
        }
    }
}
