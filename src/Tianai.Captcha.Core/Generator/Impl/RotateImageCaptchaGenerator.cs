using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public class RotateImageCaptchaGenerator : AbstractImageCaptchaGenerator
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

        int x = background.Width / 2 - fixedTemplate.Width / 2;
        int y = background.Height / 2 - fixedTemplate.Height / 2;

        // Cut image from background
        var cutImage = CaptchaImageUtils.CutImage(background, fixedTemplate, x, y);

        // Overlay fixed template on background (with optional random rotation)
        if (param.Obfuscate)
        {
            int randomDegree = RandomInt(10, 350);
            using var rotateFixed = CaptchaImageUtils.RotateImage(fixedTemplate, randomDegree);
            CaptchaImageUtils.OverlayImage(background, rotateFixed, x, y);
        }
        else
        {
            CaptchaImageUtils.OverlayImage(background, fixedTemplate, x, y);
        }

        // Generate random rotation angle
        int randomX = RandomInt(fixedTemplate.Width + 10, Math.Max(fixedTemplate.Width + 11, background.Width - 10));
        double degree = 360d - randomX / ((double)background.Width / 360d);
        degree = (degree + RandomInt(0, 360) / 10.0) % 360;
        if (degree < 0)
        {
            degree += 360;
        }

        // Create matrix template
        var matrixTemplate = CaptchaImageUtils.CreateTransparentImage(cutImage.Width, background.Height);
        
        // Process cut image with active template and mask
        var processedImage = cutImage;
        bool disposeProcessedImage = false;
        
        try
        {
            // Apply active template
            if (activeTemplate != null)
            {
                var activeImage = CaptchaImageUtils.CreateTransparentImage(cutImage.Width, cutImage.Height);
                CaptchaImageUtils.OverlayImage(activeImage, cutImage, 0, 0);
                CaptchaImageUtils.OverlayImage(activeImage, activeTemplate, 0, 0);
                processedImage = activeImage;
                disposeProcessedImage = true;
            }
            
            // Apply mask if available
            if (maskTemplate != null)
            {
                var maskedImage = CaptchaImageUtils.CreateTransparentImage(processedImage.Width, processedImage.Height);
                CaptchaImageUtils.OverlayImage(maskedImage, processedImage, 0, 0);
                CaptchaImageUtils.OverlayImage(maskedImage, maskTemplate, 0, 0);
                
                if (disposeProcessedImage)
                {
                    processedImage.Dispose();
                }
                
                processedImage = maskedImage;
                disposeProcessedImage = true;
            }
            
            // Rotate and overlay to matrix template
            CaptchaImageUtils.CenterOverlayAndRotateImage(matrixTemplate, processedImage, degree);
        }
        finally
        {
            if (disposeProcessedImage)
            {
                processedImage.Dispose();
            }
            cutImage.Dispose();
        }

        exchange.BackgroundImage = background;
        exchange.TemplateImage = matrixTemplate;
        exchange.TransferData = new RotateTransferData(degree, randomX);
    }

    protected override ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var data = (RotateTransferData)exchange.TransferData!;
        var transform = GetImageTransform()!;

        var transformData = transform.Transform(param, exchange.BackgroundImage, exchange.TemplateImage,
            exchange.ResourceImage, exchange.TemplateResource, exchange.CustomData);

        var info = RotateImageCaptchaInfo.Of(
            data.Degree,
            transformData.BackgroundImageUrl, transformData.TemplateImageUrl,
            exchange.ResourceImage?.Tag, exchange.TemplateResource?.Tag,
            exchange.BackgroundImage!.Width, exchange.BackgroundImage.Height,
            exchange.TemplateImage!.Width, exchange.TemplateImage.Height,
            data.RandomX, param.CaptchaType, exchange.CustomData);

        exchange.BackgroundImage?.Dispose();
        exchange.TemplateImage?.Dispose();
        exchange.BackgroundImage = null;
        exchange.TemplateImage = null;

        return info;
    }

    private record RotateTransferData(double Degree, int RandomX);
}
