using System.Globalization;

namespace Tianai.Captcha.Core.Common;

public class AnyMap : Dictionary<string, object?>
{
    public AnyMap() : base(StringComparer.Ordinal) { }

    public AnyMap(IDictionary<string, object?> dictionary) : base(dictionary, StringComparer.Ordinal) { }

    // Type-safe getters

    public float? GetFloat(string key, float? defaultValue = null)
    {
        return ConvertToNumber(key, defaultValue, n => Convert.ToSingle(n), s => float.Parse(s, CultureInfo.InvariantCulture));
    }

    public int? GetInt(string key, int? defaultValue = null)
    {
        return ConvertToNumber(key, defaultValue, n => Convert.ToInt32(n), s => int.Parse(s, CultureInfo.InvariantCulture));
    }

    public long? GetLong(string key, long? defaultValue = null)
    {
        return ConvertToNumber(key, defaultValue, n => Convert.ToInt64(n), s => long.Parse(s, CultureInfo.InvariantCulture));
    }

    public double? GetDouble(string key, double? defaultValue = null)
    {
        return ConvertToNumber(key, defaultValue, n => Convert.ToDouble(n), s => double.Parse(s, CultureInfo.InvariantCulture));
    }

    public bool? GetBoolean(string key, bool? defaultValue = null)
    {
        if (!TryGetValue(key, out var data) || data is null) return defaultValue;
        if (data is bool b) return b;
        if (data is string s) return bool.TryParse(s, out var result) ? result : defaultValue;
        if (data is IConvertible c) return c.ToInt32(CultureInfo.InvariantCulture) != 0;
        return defaultValue;
    }

    public string? GetString(string key, string? defaultValue = null)
    {
        if (!TryGetValue(key, out var data) || data is null) return defaultValue;
        return data is string s ? s : data.ToString() ?? defaultValue;
    }

    // ParamKey methods

    public void AddParam<T>(IParamKey<T> paramKey, T value)
    {
        this[paramKey.Key] = value;
    }

    public T? GetParam<T>(IParamKey<T> paramKey, T? defaultValue = default)
    {
        if (TryGetValue(paramKey.Key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }

    /// <summary>
    /// 获取参数值，如果不存在或类型不匹配则返回 defaultValue (对应 Java getOrDefault)
    /// </summary>
    public T GetOrDefault<T>(IParamKey<T> paramKey, T defaultValue)
    {
        if (TryGetValue(paramKey.Key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }

    public object? RemoveParam<T>(IParamKey<T> paramKey)
    {
        if (Remove(paramKey.Key, out var value))
            return value;
        return null;
    }

    // Fluent API

    public AnyMap Set(string key, object? value)
    {
        this[key] = value;
        return this;
    }

    public AnyMap Set<T>(IParamKey<T> paramKey, T value)
    {
        this[paramKey.Key] = value;
        return this;
    }

    // Factory

    public static AnyMap Of(IDictionary<string, object?> map) => new(map);
    public static AnyMap Create() => new();

    // Private helper

    private T? ConvertToNumber<T>(string key, T? defaultValue, Func<object, T> converter, Func<string, T> parser)
        where T : struct
    {
        if (!TryGetValue(key, out var data) || data is null) return defaultValue;
        if (data is T typed) return typed;
        try
        {
            if (data is IConvertible) return converter(data);
            if (data is string s) return parser(s);
        }
        catch
        {
            // ignore conversion failures
        }
        return defaultValue;
    }
}
