namespace Tianai.Captcha.Core.Common;

public interface IParamKey<T>
{
    string Key { get; }
}

public class ParamKey<T> : IParamKey<T>
{
    public string Key { get; }

    public ParamKey(string key)
    {
        Key = key;
    }
}
