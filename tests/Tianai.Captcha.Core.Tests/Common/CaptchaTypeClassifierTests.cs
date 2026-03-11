using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Tests.Common;

public class CaptchaTypeClassifierTests
{
    [Theory]
    [InlineData("SLIDER", true)]
    [InlineData("ROTATE", true)]
    [InlineData("CONCAT", true)]
    [InlineData("WordImageClick", false)]
    [InlineData("UNKNOWN", false)]
    public void IsSliderCaptcha_ClassifiesCorrectly(string type, bool expected)
    {
        Assert.Equal(expected, CaptchaTypeClassifier.IsSliderCaptcha(type));
    }

    [Theory]
    [InlineData("WordImageClick", true)]
    [InlineData("SLIDER", false)]
    [InlineData("ROTATE", false)]
    public void IsClickCaptcha_ClassifiesCorrectly(string type, bool expected)
    {
        Assert.Equal(expected, CaptchaTypeClassifier.IsClickCaptcha(type));
    }

    [Theory]
    [InlineData("Concat", true)]
    [InlineData("SLIDER", false)]
    public void IsJigsawCaptcha_ClassifiesCorrectly(string type, bool expected)
    {
        Assert.Equal(expected, CaptchaTypeClassifier.IsJigsawCaptcha(type));
    }

    [Fact]
    public void IsSliderCaptcha_CaseInsensitive()
    {
        Assert.True(CaptchaTypeClassifier.IsSliderCaptcha("slider"));
        Assert.True(CaptchaTypeClassifier.IsSliderCaptcha("Slider"));
        Assert.True(CaptchaTypeClassifier.IsSliderCaptcha("SLIDER"));
    }
}
