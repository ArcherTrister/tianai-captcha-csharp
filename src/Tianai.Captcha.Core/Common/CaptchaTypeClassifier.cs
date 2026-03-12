namespace Tianai.Captcha.Core.Common;

public static class CaptchaTypeClassifier
{
    private static readonly HashSet<CaptchaType> SliderTypes = new()
    {
        CaptchaType.Slider,
        CaptchaType.Rotate,
        CaptchaType.Concat
    };

    private static readonly HashSet<CaptchaType> ClickTypes = new()
    {
        CaptchaType.WordImageClick
    };

    // todo: 是否需要字符串类型的判断
    public static bool IsSliderCaptcha(string type)
    {
        return Enum.TryParse<CaptchaType>(type, true, out var captchaType) && SliderTypes.Contains(captchaType);
    }

    // todo: 是否需要字符串类型的判断
    public static bool IsClickCaptcha(string type)
    {
        return Enum.TryParse<CaptchaType>(type, true, out var captchaType) && ClickTypes.Contains(captchaType);
    }

    // todo: 是否需要字符串类型的判断
    public static bool IsJigsawCaptcha(string type)
    {
        return Enum.TryParse<CaptchaType>(type, true, out var captchaType) && captchaType == CaptchaType.Concat;
    }

    public static bool IsSliderCaptcha(CaptchaType type) => SliderTypes.Contains(type);

    public static bool IsClickCaptcha(CaptchaType type) => ClickTypes.Contains(type);

    public static bool IsJigsawCaptcha(CaptchaType type) => type == CaptchaType.Concat;
}
