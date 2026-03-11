namespace Tianai.Captcha.Core.Common;

public class FontWrapper
{
    public const float DefaultFontSize = 25f;

    public string Tag { get; set; } = CommonConstant.DefaultTag;
    public CaptchaResource? FontResource { get; set; }
    public object? Font { get; set; }

    public FontWrapper() { }

    public FontWrapper(string tag, CaptchaResource? fontResource, object? font)
    {
        Tag = tag;
        FontResource = fontResource;
        Font = font;
    }
}
