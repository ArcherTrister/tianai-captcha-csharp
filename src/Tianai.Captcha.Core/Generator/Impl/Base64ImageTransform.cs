using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator.Impl;

public class Base64ImageTransform : IImageTransform
{
    public ImageTransformData Transform(GenerateParam param, SKBitmap? backgroundImage, SKBitmap? templateImage,
                                         CaptchaResource? backgroundResource, ResourceMap? templateResource,
                                         CustomData data)
    {
        var result = new ImageTransformData();
        var bgFormat = CaptchaImageUtils.GetImageFormat(param.BackgroundFormatName);
        var tplFormat = CaptchaImageUtils.GetImageFormat(param.TemplateFormatName);

        if (backgroundImage != null)
        {
            var bgBitmap = CaptchaImageUtils.IsJpeg(param.BackgroundFormatName)
                ? CaptchaImageUtils.ToBufferedImage(backgroundImage, param.BackgroundFormatName)
                : backgroundImage;

            using var bgImg = SKImage.FromBitmap(bgBitmap);
            using var bgData = bgImg.Encode(bgFormat, 80)
                ?? throw new ImageCaptchaException($"Failed to encode background image as {param.BackgroundFormatName}");
            result.BackgroundImageUrl = $"data:image/{param.BackgroundFormatName};base64,{Convert.ToBase64String(bgData.ToArray())}";

            if (!ReferenceEquals(bgBitmap, backgroundImage))
                bgBitmap.Dispose();
        }

        if (templateImage != null)
        {
            using var tplImg = SKImage.FromBitmap(templateImage);
            using var tplData = tplImg.Encode(tplFormat, 80)
                ?? throw new ImageCaptchaException($"Failed to encode template image as {param.TemplateFormatName}");
            result.TemplateImageUrl = $"data:image/{param.TemplateFormatName};base64,{Convert.ToBase64String(tplData.ToArray())}";
        }

        return result;
    }
}
