using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

internal struct LevelKey 
{
    internal int Level;
    internal string Key;
}

internal sealed class State
{
    internal IDictionary<LevelKey, JArray> ParentArray { get; set; }

    internal IDictionary<LevelKey, JToken> CurrentArrayToken { get; set; }

    internal IDictionary<LevelKey, JToken> CurrentScopeToken { get; set; }

    // internal State(IDictionary<string, JArray> parentArray, IDictionary<string, JToken> currentArrayToken, IDictionary<string, JToken> currentScopeToken)
    // {
    //     this.ParentArray = parentArray;
    //     this.CurrentArrayToken = currentArrayToken;
    //     this.CurrentScopeToken = currentScopeToken;
    // }

    internal JToken GetLastLevelToken()
    {
        if ((CurrentArrayToken?.Max(k => k.Key.Level) ?? 0) > (CurrentScopeToken?.Max(k => k.Key.Level) ?? 0))
        {
            return CurrentArrayToken?.Last().Value;
        }
        return CurrentScopeToken?.Last().Value;
    }
}