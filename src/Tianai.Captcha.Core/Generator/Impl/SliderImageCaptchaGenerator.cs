using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public class SliderImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    public const string TemplateActiveImageName = "active.png";
    public const string TemplateFixedImageName = "fixed.png";
    public const string TemplateMaskImageName = "mask.png";

    protected override void DoInit() { }

    protected override void DoGenerateCaptchaImage(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var templateResource = RequiredRandomGetTemplate(param.CaptchaType, param.TemplateImageTag);
        var resourceImage = RequiredRandomGetResource(param.CaptchaType, param.BackgroundImageTag);

        exchange.TemplateResource = templateResource;
        exchange.ResourceImage = resourceImage;

        using var fixedTemplate = GetTemplateImage(templateResource, TemplateFixedImageName);
        using var activeTemplate = GetTemplateImage(templateResource, TemplateActiveImageName);
        using var maskTemplate = GetTemplateImageOrNull(templateResource, TemplateMaskImageName);
        var background = GetResourceImage(resourceImage);

        int randomX = RandomInt(fixedTemplate.Width + 5, Math.Max(fixedTemplate.Width + 6, background.Width - fixedTemplate.Width - 10));
        int randomY = RandomInt(0, Math.Max(1, background.Height - fixedTemplate.Height));

        // Cut the slider from background using fixed template as mask
        var cutImage = CaptchaImageUtils.CutImage(background, fixedTemplate, randomX, randomY);

        // Overlay the fixed template (hole) on background
        CaptchaImageUtils.OverlayImage(background, fixedTemplate, randomX, randomY);

        // Handle obfuscation if enabled
        if (param.Obfuscate)
        {
            using var obfuscateImage = CreateObfuscate(fixedTemplate);
            int obfuscateX = RandomObfuscateX(randomX, fixedTemplate.Width, background.Width);
            CaptchaImageUtils.OverlayImage(background, obfuscateImage, obfuscateX, randomY);
        }

        // Process cut image with active template and mask
        var processedImage = cutImage;
        bool disposeProcessedImage = false;
        
        try
        {
            // Apply mask if available
            if (maskTemplate != null)
            {
                var maskedImage = CaptchaImageUtils.CreateTransparentImage(processedImage.Width, processedImage.Height);
                CaptchaImageUtils.OverlayImage(maskedImage, processedImage, 0, 0);
                CaptchaImageUtils.OverlayImage(maskedImage, maskTemplate, 0, 0);
                
                processedImage = maskedImage;
                disposeProcessedImage = true;
            }
            
            // Overlay active template on processed image
            CaptchaImageUtils.OverlayImage(processedImage, activeTemplate, 0, 0);

            // Create the template image (full height, slider at randomY position)
            var matrixTemplate = CaptchaImageUtils.CreateTransparentImage(activeTemplate.Width, background.Height);
            CaptchaImageUtils.OverlayImage(matrixTemplate, processedImage, 0, randomY);
            
            exchange.BackgroundImage = background;
            exchange.TemplateImage = matrixTemplate;
            exchange.TransferData = new SliderTransferData(randomX, randomY);
        }
        finally
        {
            if (disposeProcessedImage)
            {
                processedImage.Dispose();
            }
            cutImage.Dispose();
        }
    }

    protected override ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var data = (SliderTransferData)exchange.TransferData!;
        var transform = GetImageTransform()!;

        var transformData = transform.Transform(param, exchange.BackgroundImage, exchange.TemplateImage,
            exchange.ResourceImage, exchange.TemplateResource, exchange.CustomData);

        var info = SliderImageCaptchaInfo.Of(
            data.X, data.Y,
            transformData.BackgroundImageUrl, transformData.TemplateImageUrl,
            exchange.ResourceImage?.Tag, exchange.TemplateResource?.Tag,
            exchange.BackgroundImage!.Width, exchange.BackgroundImage.Height,
            exchange.TemplateImage!.Width, exchange.TemplateImage.Height,
            param.CaptchaType, exchange.CustomData);

        // Dispose bitmaps after transform
        exchange.BackgroundImage?.Dispose();
        exchange.TemplateImage?.Dispose();
        exchange.BackgroundImage = null;
        exchange.TemplateImage = null;

        return info;
    }

    private SKBitmap CreateObfuscate(SKBitmap fixedImage)
    {
        float scale = 0.6f + (float)Random.Shared.NextDouble() * 0.4f;
        int newW = (int)(fixedImage.Width * scale);
        int newH = (int)(fixedImage.Height * scale);

        var info = new SKImageInfo(newW, newH, SKColorType.Rgba8888, SKAlphaType.Premul);
        var resized = fixedImage.Resize(info, SKSamplingOptions.Default);
        return resized ?? fixedImage.Copy();
    }

    private int RandomObfuscateX(int randomX, int templateWidth, int bgWidth)
    {
        int halfW = templateWidth / 2;
        int obfuscateX;
        if (randomX > bgWidth / 2)
        {
            obfuscateX = RandomInt(halfW, Math.Max(halfW + 1, randomX - templateWidth));
        }
        else
        {
            obfuscateX = RandomInt(randomX + templateWidth, Math.Max(randomX + templateWidth + 1, bgWidth - halfW));
        }
        return obfuscateX;
    }

    private record SliderTransferData(int X, int Y);
}
