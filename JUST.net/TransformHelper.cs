using System.Collections.Generic;
using Newtonsoft.Json.Linq;

internal class TransformHelper
{
    internal IList<JToken> selectedTokens;
    internal IDictionary<string, JToken> tokensToReplace;
    internal IList<JToken> tokensToDelete;
    internal IList<string> loopProperties;
    internal IList<string> condProps;
    internal JArray arrayToForm;
    internal JObject dictToForm;
    internal IList<JToken> tokenToForm;
    internal IList<JToken> tokensToAdd;
    internal bool isLoop;
    internal bool isBulk;
}