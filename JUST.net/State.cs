using System.Collections.Generic;
using Newtonsoft.Json.Linq;

internal sealed class State
{
    internal IDictionary<string, JArray> ParentArray { get; set; }

    internal IDictionary<string, JToken> CurrentArrayToken { get; set; }

    internal IDictionary<string, JToken> CurrentScopeToken { get; set; }

    // internal State(IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, IDictionary<string, JToken> currentScopeToken)
    // {
    //     this.ParentArray = parentArray;
    //     this.CurrentArrayToken = currentArrayToken;
    //     this.CurrentScopeToken = currentScopeToken;
    // }
}