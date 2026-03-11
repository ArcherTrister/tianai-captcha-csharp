using SkiaSharp;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Common;

public class CaptchaExchange
{
    public ResourceMap? TemplateResource { get; set; }
    public CaptchaResource? ResourceImage { get; set; }
    public SKBitmap? BackgroundImage { get; set; }
    public SKBitmap? TemplateImage { get; set; }
    public CustomData CustomData { get; set; } = new();
    public GenerateParam Param { get; set; } = new();
    public object? TransferData { get; set; }

    public static CaptchaExchange Create(CustomData customData, GenerateParam param)
    {
        return new CaptchaExchange
        {
            CustomData = customData,
            Param = param
        };
    }
}
