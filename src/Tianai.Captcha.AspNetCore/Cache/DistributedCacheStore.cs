using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.AspNetCore.Cache;

public class DistributedCacheStore : ICacheStore
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public DistributedCacheStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public AnyMap? GetCache(string key)
    {
        var json = _cache.GetString(key);
        if (json == null) return null;
        return Deserialize(json);
    }

    public AnyMap? GetAndRemoveCache(string key)
    {
        var json = _cache.GetString(key);
        if (json == null) return null;
        _cache.Remove(key);
        return Deserialize(json);
    }

    public bool SetCache(string key, AnyMap data, long expire, TimeUnit timeUnit)
    {
        var json = Serialize(data);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeUnit.ToTimeSpan(expire)
        };
        _cache.SetString(key, json, options);
        return true;
    }

    public long Incr(string key, long delta, long expire, TimeUnit timeUnit)
    {
        // IDistributedCache doesn't support atomic increment.
        // Using get-increment-set with optimistic approach.
        var current = GetLong(key) ?? 0;
        var newValue = current + delta;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeUnit.ToTimeSpan(expire)
        };
        _cache.SetString(key, newValue.ToString(), options);
        return newValue;
    }

    public long? GetLong(string key)
    {
        var str = _cache.GetString(key);
        if (str != null && long.TryParse(str, out var value))
            return value;
        return null;
    }

    public void Dispose() { }

    private static string Serialize(AnyMap data)
    {
        var dict = new Dictionary<string, object?>(data);
        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static AnyMap Deserialize(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions);
        if (dict == null) return AnyMap.Create();

        var map = AnyMap.Create();
        foreach (var kvp in dict)
        {
            map[kvp.Key] = ConvertJsonElement(kvp.Value);
        }
        return map;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var i) => i,
            JsonValueKind.Number when element.TryGetInt64(out var l) => l,
            JsonValueKind.Number when element.TryGetDouble(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }
}
