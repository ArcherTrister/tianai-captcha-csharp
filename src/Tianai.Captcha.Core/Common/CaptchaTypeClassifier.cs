namespace Tianai.Captcha.Core.Common;

public static class CaptchaTypeClassifier
{
    private static readonly HashSet<string> SliderTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        CaptchaType.Slider.ToString(),
        CaptchaType.Rotate.ToString(),
        CaptchaType.Concat.ToString()
    };

    private static readonly HashSet<string> ClickTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        CaptchaType.WordImageClick.ToString()
    };

    public static bool IsSliderCaptcha(string type) => SliderTypes.Contains(type);

    public static bool IsClickCaptcha(string type) => ClickTypes.Contains(type);

    public static bool IsJigsawCaptcha(string type) =>
        CaptchaType.Concat.ToString().Equals(type, StringComparison.OrdinalIgnoreCase);
}
