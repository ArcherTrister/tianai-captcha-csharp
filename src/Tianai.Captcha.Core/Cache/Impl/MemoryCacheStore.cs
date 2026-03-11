using Microsoft.Extensions.Caching.Memory;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Cache.Impl;

public class MemoryCacheStore : ICacheStore
{
    private readonly IMemoryCache _cache;
    private readonly object _lockObj = new();

    public MemoryCacheStore() : this(new MemoryCache(new MemoryCacheOptions())) { }

    public MemoryCacheStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public AnyMap? GetCache(string key)
    {
        return _cache.TryGetValue(key, out AnyMap? value) ? value : null;
    }

    public AnyMap? GetAndRemoveCache(string key)
    {
        if (_cache.TryGetValue(key, out AnyMap? value))
        {
            _cache.Remove(key);
            return value;
        }
        return null;
    }

    public bool SetCache(string key, AnyMap data, long expire, TimeUnit timeUnit)
    {
        var expiration = timeUnit.ToTimeSpan(expire);
        _cache.Set(key, data, expiration);
        return true;
    }

    public long Incr(string key, long delta, long expire, TimeUnit timeUnit)
    {
        lock (_lockObj)
        {
            var current = _cache.TryGetValue(key, out long existing) ? existing : 0L;
            var newValue = current + delta;
            var expiration = timeUnit.ToTimeSpan(expire);
            _cache.Set(key, newValue, expiration);
            return newValue;
        }
    }

    public long? GetLong(string key)
    {
        return _cache.TryGetValue(key, out long value) ? value : null;
    }

    public void Dispose()
    {
        if (_cache is MemoryCache mc)
            mc.Dispose();
    }
}
