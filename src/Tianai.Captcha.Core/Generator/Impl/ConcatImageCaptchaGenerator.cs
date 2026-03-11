using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public class ConcatImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    protected override void DoInit() { }

    protected override void DoGenerateCaptchaImage(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var resourceImage = RequiredRandomGetResource(param.CaptchaType, param.BackgroundImageTag);
        exchange.ResourceImage = resourceImage;

        var bgImage = GetResourceImage(resourceImage);

        // Split vertically first
        int spacingY = bgImage.Height / 4;
        int randomY = RandomInt(spacingY, Math.Max(spacingY + 1, bgImage.Height - spacingY));
        var bgImageSplit = CaptchaImageUtils.SplitImage(randomY, true, bgImage);

        // Split top part horizontally
        int spacingX = bgImage.Width / 8;
        int randomX = RandomInt(spacingX, Math.Max(spacingX + 1, bgImage.Width - bgImage.Width / 5));
        var bgImageTopSplit = CaptchaImageUtils.SplitImage(randomX, false, bgImageSplit[0]);

        // Concat horizontally (swap left and right)
        var sliderImage = CaptchaImageUtils.ConcatImage(true,
            bgImageTopSplit[0].Width + bgImageTopSplit[1].Width,
            bgImageTopSplit[0].Height,
            bgImageTopSplit[1], bgImageTopSplit[0]);

        // Concat vertically (slider on top, bottom part below)
        var finalImage = CaptchaImageUtils.ConcatImage(false,
            bgImageSplit[1].Width,
            sliderImage.Height + bgImageSplit[1].Height,
            sliderImage, bgImageSplit[1]);

        // Cleanup
        bgImage.Dispose();
        bgImageSplit[0].Dispose();
        bgImageSplit[1].Dispose();
        bgImageTopSplit[0].Dispose();
        bgImageTopSplit[1].Dispose();
        sliderImage.Dispose();

        exchange.BackgroundImage = finalImage;
        exchange.TransferData = new ConcatTransferData(randomX, randomY);
        // SDK requires randomY in ViewData to calculate display split position
        exchange.CustomData.ViewData["randomY"] = randomY;
    }

    protected override ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var data = (ConcatTransferData)exchange.TransferData!;
        var transform = GetImageTransform()!;

        var transformData = transform.Transform(param, exchange.BackgroundImage, null,
            exchange.ResourceImage, null, exchange.CustomData);

        var info = SliderImageCaptchaInfo.Of(
            data.X, data.Y,
            transformData.BackgroundImageUrl, null,
            exchange.ResourceImage?.Tag, null,
            exchange.BackgroundImage!.Width, exchange.BackgroundImage.Height,
            0, 0,
            param.CaptchaType, exchange.CustomData);

        exchange.BackgroundImage?.Dispose();
        exchange.BackgroundImage = null;

        return info;
    }

    private record ConcatTransferData(int X, int Y);
}
