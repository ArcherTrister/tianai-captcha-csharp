namespace Tianai.Captcha.Core.Common;

public enum CaptchaImageType
{
    WebP,
    JpegPng
}

public static class CaptchaImageTypeExtensions
{
    public static CaptchaImageType GetType(string bgImageType, string templateImageType)
    {
        if ("webp".Equals(bgImageType, StringComparison.OrdinalIgnoreCase)
            && "webp".Equals(templateImageType, StringComparison.OrdinalIgnoreCase))
        {
            return CaptchaImageType.WebP;
        }
        return CaptchaImageType.JpegPng;
    }
}
