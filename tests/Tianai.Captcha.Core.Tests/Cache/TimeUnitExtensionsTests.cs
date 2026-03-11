using Tianai.Captcha.Core.Cache;

namespace Tianai.Captcha.Core.Tests.Cache;

public class TimeUnitExtensionsTests
{
    [Theory]
    [InlineData(TimeUnit.Milliseconds, 500, 500)]
    [InlineData(TimeUnit.Seconds, 30, 30_000)]
    [InlineData(TimeUnit.Minutes, 5, 300_000)]
    [InlineData(TimeUnit.Hours, 2, 7_200_000)]
    [InlineData(TimeUnit.Days, 1, 86_400_000)]
    public void ToTimeSpan_ConvertsCorrectly(TimeUnit unit, long value, double expectedMilliseconds)
    {
        var result = unit.ToTimeSpan(value);
        Assert.Equal(expectedMilliseconds, result.TotalMilliseconds);
    }
}
