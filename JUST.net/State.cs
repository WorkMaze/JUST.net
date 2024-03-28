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
    internal const string RootKey = "root";
    internal State(JToken transformer, JToken input, int levelCounter,
        IDictionary<LevelKey, JToken> currentArrayToken = null,
        IDictionary<LevelKey, JToken> currentScopeToken = null,
        bool multiple = false)
    {
        Transformer = transformer;
        ParentArray = new Dictionary<LevelKey, JArray>();
        CurrentArrayToken = new Dictionary<LevelKey, JToken> { { new LevelKey { Level = levelCounter, Key = State.RootKey}, input } }
            .Concat(currentArrayToken ?? new Dictionary<LevelKey, JToken>()).ToDictionary(p => p.Key, p => p.Value);
        CurrentScopeToken = new Dictionary<LevelKey, JToken> { { new LevelKey { Level = levelCounter, Key = State.RootKey}, input } }
            .Concat(currentScopeToken ?? new Dictionary<LevelKey, JToken>()).ToDictionary(p => p.Key, p => p.Value);
        Multiple = multiple;
    }
    internal JToken Transformer { get; private set; }
    internal IDictionary<LevelKey, JArray> ParentArray { get; private set; }
    internal IDictionary<LevelKey, JToken> CurrentArrayToken { get; private set; }
    internal IDictionary<LevelKey, JToken> CurrentScopeToken { get; private set; }
    internal bool Multiple { get; private set; }

    internal string GetHigherAlias()
    {
        if (IsArrayHigherThanScope())
        {
            return CurrentArrayToken.Last().Key.Key;
        }
        return CurrentScopeToken.Last().Key.Key;
    }

    internal JToken GetAliasToken(string alias)
    {
        IEnumerable<KeyValuePair<LevelKey, JToken>> arr = CurrentArrayToken.Where(a => a.Key.Key == alias);
        IEnumerable<KeyValuePair<LevelKey, JToken>> scope = CurrentScopeToken.Where(a => a.Key.Key == alias);

        int maxArr = arr.Any() ? arr.Max(a => a.Key.Level) : -1;
        int maxScope = scope.Any() ? scope.Max(a => a.Key.Level) : -1;

        if (maxArr > maxScope)
        {
            return arr.Single(a => a.Key.Level == maxArr).Value;
        }
        return scope.Single(a => a.Key.Level == maxScope).Value;
    }

    private bool IsArrayHigherThanScope()
    {
        return (CurrentArrayToken?.Max(k => k.Key.Level) ?? 0) > (CurrentScopeToken?.Max(k => k.Key.Level) ?? 0);
    }
}