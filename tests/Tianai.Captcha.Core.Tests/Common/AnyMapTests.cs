using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Tests.Common;

public class AnyMapTests
{
    [Fact]
    public void GetInt_WithIntValue_ReturnsInt()
    {
        var map = new AnyMap { ["key"] = 42 };
        Assert.Equal(42, map.GetInt("key"));
    }

    [Fact]
    public void GetInt_WithMissingKey_ReturnsDefault()
    {
        var map = new AnyMap();
        Assert.Null(map.GetInt("missing"));
        Assert.Equal(99, map.GetInt("missing", 99));
    }

    [Fact]
    public void GetFloat_WithDoubleValue_ConvertsCorrectly()
    {
        var map = new AnyMap { ["key"] = 3.14 };
        var result = map.GetFloat("key");
        Assert.NotNull(result);
        Assert.Equal(3.14f, result.Value, 0.01f);
    }

    [Fact]
    public void GetString_WithStringValue_ReturnsString()
    {
        var map = new AnyMap { ["key"] = "hello" };
        Assert.Equal("hello", map.GetString("key"));
    }

    [Fact]
    public void GetString_WithNonStringValue_CallsToString()
    {
        var map = new AnyMap { ["key"] = 42 };
        Assert.Equal("42", map.GetString("key"));
    }

    [Fact]
    public void GetBoolean_WithBoolValue_ReturnsBool()
    {
        var map = new AnyMap { ["key"] = true };
        Assert.True(map.GetBoolean("key"));
    }

    [Fact]
    public void GetBoolean_WithStringTrue_ReturnsTrueForParsableString()
    {
        var map = new AnyMap { ["key"] = "true" };
        Assert.True(map.GetBoolean("key"));
    }

    [Fact]
    public void GetLong_WithLongValue_ReturnsLong()
    {
        var map = new AnyMap { ["key"] = 12345678901234L };
        Assert.Equal(12345678901234L, map.GetLong("key"));
    }

    [Fact]
    public void GetDouble_WithIntValue_Converts()
    {
        var map = new AnyMap { ["key"] = 42 };
        Assert.Equal(42.0, map.GetDouble("key"));
    }

    [Fact]
    public void Set_FluentApi_ReturnsSelf()
    {
        var map = new AnyMap()
            .Set("a", 1)
            .Set("b", "test");

        Assert.Equal(1, map["a"]);
        Assert.Equal("test", map["b"]);
    }

    [Fact]
    public void ParamKey_AddAndGet_Works()
    {
        var key = new ParamKey<int>("myInt");
        var map = new AnyMap();
        map.AddParam(key, 42);
        Assert.Equal(42, map.GetParam(key));
    }

    [Fact]
    public void ParamKey_Remove_RemovesEntry()
    {
        var key = new ParamKey<string>("str");
        var map = new AnyMap();
        map.AddParam(key, "hello");
        map.RemoveParam(key);
        Assert.Equal(default, map.GetParam(key));
    }

    [Fact]
    public void Create_ReturnsEmptyMap()
    {
        var map = AnyMap.Create();
        Assert.Empty(map);
    }

    [Fact]
    public void Of_CopiesDictionary()
    {
        var dict = new Dictionary<string, object?> { ["a"] = 1, ["b"] = "test" };
        var map = AnyMap.Of(dict);
        Assert.Equal(1, map["a"]);
        Assert.Equal("test", map["b"]);
    }

    [Fact]
    public void GetInt_WithStringValue_ParsesCorrectly()
    {
        var map = new AnyMap { ["key"] = "123" };
        Assert.Equal(123, map.GetInt("key"));
    }

    [Fact]
    public void GetFloat_WithNullValue_ReturnsDefault()
    {
        var map = new AnyMap { ["key"] = null };
        Assert.Null(map.GetFloat("key"));
        Assert.Equal(1.5f, map.GetFloat("key", 1.5f));
    }
}
