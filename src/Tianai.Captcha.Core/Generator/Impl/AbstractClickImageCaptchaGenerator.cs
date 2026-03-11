using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public abstract class AbstractClickImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    public int CheckClickCount { get; set; } = 4;
    public int InterferenceCount { get; set; } = 2;

    protected override void DoInit() { }

    protected override void DoGenerateCaptchaImage(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var resourceImage = RequiredRandomGetResource(param.CaptchaType, param.BackgroundImageTag);
        exchange.ResourceImage = resourceImage;

        var bgImage = GetResourceImage(resourceImage);
        var allCheckDefinitions = new List<ClickImageCheckDefinition>();

        // 从 param 中读取运行时参数，支持按请求覆盖 (对应 Java ParamKeyEnum)
        var checkCount = param.GetOrDefault(ParamKeyEnum.ClickCheckClickCount, CheckClickCount);
        var interferenceCount = param.GetOrDefault(ParamKeyEnum.ClickInterferenceCount, InterferenceCount);

        // Get all click items (word images, etc.)
        var totalCount = checkCount + interferenceCount;
        for (int i = 0; i < totalCount; i++)
        {
            var tip = GetRandomTip(param);
            var randomColor = CaptchaImageUtils.GetRandomColor(Random.Shared);
            var imgWrapper = GetClickImg(param, tip, randomColor, bgImage);

            var definition = new ClickImageCheckDefinition
            {
                Tip = tip,
                TipImage = imgWrapper,
                ImageColor = randomColor
            };
            allCheckDefinitions.Add(definition);
        }

        // Place images on background
        PlaceClickImages(bgImage, allCheckDefinitions);

        // Filter and sort check definitions
        var checkDefinitions = FilterAndSortClickImageCheckDefinition(exchange, allCheckDefinitions);

        exchange.BackgroundImage = bgImage;
        exchange.TransferData = new ClickTransferData(allCheckDefinitions, checkDefinitions);
    }

    protected virtual void PlaceClickImages(SKBitmap bgImage, List<ClickImageCheckDefinition> definitions)
    {
        int bgW = bgImage.Width;
        int bgH = bgImage.Height;
        var usedAreas = new List<(int x, int y, int w, int h)>();

        foreach (var def in definitions)
        {
            if (def.TipImage?.Image == null) continue;
            var img = def.TipImage.Image;
            int imgW = img.Width;
            int imgH = img.Height;

            int x, y;
            int attempts = 0;
            do
            {
                x = Random.Shared.Next(5, Math.Max(6, bgW - imgW - 5));
                y = Random.Shared.Next(5, Math.Max(6, bgH - imgH - 5));
                attempts++;
            } while (attempts < 50 && IsOverlapping(x, y, imgW, imgH, usedAreas));

            usedAreas.Add((x, y, imgW, imgH));
            def.X = x;
            def.Y = y;
            def.Width = imgW;
            def.Height = imgH;

            CaptchaImageUtils.OverlayImage(bgImage, img, x, y);
        }
    }

    private static bool IsOverlapping(int x, int y, int w, int h, List<(int x, int y, int w, int h)> areas)
    {
        foreach (var area in areas)
        {
            if (x < area.x + area.w && x + w > area.x && y < area.y + area.h && y + h > area.y)
                return true;
        }
        return false;
    }

    protected virtual List<ClickImageCheckDefinition> FilterAndSortClickImageCheckDefinition(
        CaptchaExchange exchange, List<ClickImageCheckDefinition> allDefinitions)
    {
        var checkCount = exchange.Param.GetOrDefault(ParamKeyEnum.ClickCheckClickCount, CheckClickCount);
        var shuffled = new List<ClickImageCheckDefinition>(allDefinitions);
        var rng = Random.Shared;
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled.Take(checkCount).ToList();
    }

    protected abstract CaptchaResource GetRandomTip(GenerateParam param);
    protected abstract ClickImageCheckDefinition.ImgWrapper GetClickImg(GenerateParam param, CaptchaResource tip, SKColor color, SKBitmap bgImage);

    protected override ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var param = exchange.Param;
        var data = (ClickTransferData)exchange.TransferData!;
        var transform = GetImageTransform()!;

        // Build view data - the tips to show to user
        var viewTips = data.CheckDefinitions.Select(d => d.Tip?.Data ?? "").ToList();
        exchange.CustomData.ViewData["wordList"] = viewTips;

        // Store check definitions in internal data for validation
        exchange.CustomData.Data["checkDefinitions"] = data.CheckDefinitions;

        // Create tip image strip from check definitions (characters in click order)
        SKBitmap? tipImage = null;
        var checkDefs = data.CheckDefinitions;
        if (checkDefs.Count > 0)
        {
            int totalWidth = checkDefs.Sum(d => d.TipImage?.Image?.Width ?? 0);
            int maxHeight = checkDefs.Max(d => d.TipImage?.Image?.Height ?? 0);

            if (totalWidth > 0 && maxHeight > 0)
            {
                tipImage = CaptchaImageUtils.CreateTransparentImage(totalWidth, maxHeight);
                using var tipCanvas = new SKCanvas(tipImage);

                int offsetX = 0;
                foreach (var def in checkDefs)
                {
                    if (def.TipImage?.Image != null)
                    {
                        tipCanvas.DrawBitmap(def.TipImage.Image, offsetX, 0);
                        offsetX += def.TipImage.Image.Width;
                    }
                }
            }
        }

        var transformData = transform.Transform(param, exchange.BackgroundImage, tipImage,
            exchange.ResourceImage, null, exchange.CustomData);

        var info = new ImageCaptchaInfo
        {
            BackgroundImage = transformData.BackgroundImageUrl,
            TemplateImage = transformData.TemplateImageUrl,
            BackgroundImageTag = exchange.ResourceImage?.Tag,
            BackgroundImageWidth = exchange.BackgroundImage!.Width,
            BackgroundImageHeight = exchange.BackgroundImage.Height,
            TemplateImageWidth = tipImage?.Width ?? 0,
            TemplateImageHeight = tipImage?.Height ?? 0,
            Type = param.CaptchaType,
            Data = exchange.CustomData
        };

        // Dispose
        tipImage?.Dispose();
        foreach (var def in data.AllDefinitions)
            def.TipImage?.Image?.Dispose();
        exchange.BackgroundImage?.Dispose();
        exchange.BackgroundImage = null;

        return info;
    }

    protected record ClickTransferData(
        List<ClickImageCheckDefinition> AllDefinitions,
        List<ClickImageCheckDefinition> CheckDefinitions);
}
