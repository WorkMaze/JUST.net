using JUST;
using JUST.net.Selectables;
using Newtonsoft.Json.Linq;

public interface IContext{
    char SplitGroupChar { get; }
    int DefaultDecimalPlaces { get; }
    bool IsStrictMode();
    T Resolve<T>(JToken token) where T: ISelectableToken;
}