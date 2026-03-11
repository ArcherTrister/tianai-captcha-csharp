using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Cache.Impl;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Tests.Cache;

public class MemoryCacheStoreTests : IDisposable
{
    private readonly MemoryCacheStore _store;

    public MemoryCacheStoreTests()
    {
        _store = new MemoryCacheStore();
    }

    [Fact]
    public void SetCache_ThenGetCache_ReturnsData()
    {
        var data = new AnyMap { ["name"] = "test" };
        _store.SetCache("key1", data, 60, TimeUnit.Seconds);
        var result = _store.GetCache("key1");
        Assert.NotNull(result);
        Assert.Equal("test", result.GetString("name"));
    }

    [Fact]
    public void GetCache_MissingKey_ReturnsNull()
    {
        Assert.Null(_store.GetCache("nonexistent"));
    }

    [Fact]
    public void GetAndRemoveCache_RemovesEntry()
    {
        var data = new AnyMap { ["val"] = 42 };
        _store.SetCache("key2", data, 60, TimeUnit.Seconds);

        var result = _store.GetAndRemoveCache("key2");
        Assert.NotNull(result);
        Assert.Equal(42, result["val"]);

        // Should be gone now
        Assert.Null(_store.GetCache("key2"));
    }

    [Fact]
    public void Incr_NewKey_ReturnsDelta()
    {
        var result = _store.Incr("counter", 1, 60, TimeUnit.Seconds);
        Assert.Equal(1, result);
    }

    [Fact]
    public void Incr_ExistingKey_Accumulates()
    {
        _store.Incr("counter", 5, 60, TimeUnit.Seconds);
        _store.Incr("counter", 3, 60, TimeUnit.Seconds);
        var result = _store.Incr("counter", 2, 60, TimeUnit.Seconds);
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetLong_ReturnsStoredLong()
    {
        _store.Incr("num", 42, 60, TimeUnit.Seconds);
        Assert.Equal(42L, _store.GetLong("num"));
    }

    [Fact]
    public void GetLong_MissingKey_ReturnsNull()
    {
        Assert.Null(_store.GetLong("missing"));
    }

    public void Dispose()
    {
        _store.Dispose();
    }
}
