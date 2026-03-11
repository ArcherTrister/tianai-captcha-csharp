using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public class RotateImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    public const string TemplateActiveImageName = "active.png";
    public const string TemplateFixedImageName = "fixed.png";

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
        var background = GetResourceImage(resourceImage);

        int x = background.Width / 2 - fixedTemplate.Width / 2;
        int y = background.Height / 2 - fixedTemplate.Height / 2;

        // Cut image from background
        var cutImage = CaptchaImageUtils.CutImage(background, fixedTemplate, x, y);

        // Overlay fixed template on background (with optional random rotation)
        SKBitmap rotateFixed = fixedTemplate;
        SKBitmap rotateActive = activeTemplate;
        bool disposeRotated = false;

        if (param.Obfuscate)
        {
            int randomDegree = RandomInt(10, 350);
            rotateFixed = CaptchaImageUtils.RotateImage(fixedTemplate, randomDegree);
            rotateActive = CaptchaImageUtils.RotateImage(activeTemplate, randomDegree);
            disposeRotated = true;
        }

        CaptchaImageUtils.OverlayImage(background, rotateFixed, x, y);

        if (disposeRotated)
        {
            rotateFixed.Dispose();
            rotateActive.Dispose();
        }

        // Generate random rotation angle with enhanced randomness
        int randomX = RandomInt(fixedTemplate.Width + 10, Math.Max(fixedTemplate.Width + 11, background.Width - 10));
        // Add additional randomness to ensure different angles even with caching
        double randomOffset = RandomInt(0, 360) / 10.0; // Add up to 36 degrees of random offset
        double degree = 360d - randomX / ((double)background.Width / 360d);
        degree = (degree + randomOffset) % 360;

        // Overlay active template on cut image
        CaptchaImageUtils.OverlayImage(cutImage, activeTemplate, 0, 0);

        // Create matrix template and center overlay with rotation
        var matrixTemplate = CaptchaImageUtils.CreateTransparentImage(cutImage.Width, background.Height);
        CaptchaImageUtils.CenterOverlayAndRotateImage(matrixTemplate, cutImage, degree);
        cutImage.Dispose();

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
