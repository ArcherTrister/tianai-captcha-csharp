using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Cache;

public interface ICacheStore : IDisposable
{
    AnyMap? GetCache(string key);
    AnyMap? GetAndRemoveCache(string key);
    bool SetCache(string key, AnyMap data, long expire, TimeUnit timeUnit);
    long Incr(string key, long delta, long expire, TimeUnit timeUnit);
    long? GetLong(string key);
}

public enum TimeUnit
{
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days
}

public static class TimeUnitExtensions
{
    public static TimeSpan ToTimeSpan(this TimeUnit unit, long value)
    {
        return unit switch
        {
            TimeUnit.Milliseconds => TimeSpan.FromMilliseconds(value),
            TimeUnit.Seconds => TimeSpan.FromSeconds(value),
            TimeUnit.Minutes => TimeSpan.FromMinutes(value),
            TimeUnit.Hours => TimeSpan.FromHours(value),
            TimeUnit.Days => TimeSpan.FromDays(value),
            _ => TimeSpan.FromMilliseconds(value)
        };
    }
}
