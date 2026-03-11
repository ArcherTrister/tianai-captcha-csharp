using System.Text;
using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Generator.Impl;

public class WordClickImageCaptchaGenerator : AbstractClickImageCaptchaGenerator
{
    private SKTypeface? _typeface;

    protected override void DoInit()
    {
        base.DoInit();
        // Try to load default font
        try
        {
            var assembly = typeof(WordClickImageCaptchaGenerator).Assembly;
            var names = assembly.GetManifestResourceNames();
            var fontName = names.FirstOrDefault(n => n.EndsWith("SIMSUN.TTC", StringComparison.OrdinalIgnoreCase));
            if (fontName != null)
            {
                using var stream = assembly.GetManifestResourceStream(fontName);
                if (stream != null)
                    _typeface = SKTypeface.FromStream(stream);
            }
        }
        catch
        {
            // Ignore font loading errors
        }

        _typeface ??= SKTypeface.Default;
    }

    protected override CaptchaResource GetRandomTip(GenerateParam param)
    {
        var word = GetRandomChar(Random.Shared);
        return new CaptchaResource("word", word);
    }

    /// <summary>
    /// 随机生成一个汉字，对应 Java FontUtils.getRandomChar()。
    /// 使用 GB2312 编码的高低位随机生成，覆盖常用汉字区。
    /// 高位: 0xB0-0xD6 (176-214), 低位: 0xA1-0xFD (161-253)
    /// </summary>
    private static string GetRandomChar(Random random)
    {
        int highPos = 176 + random.Next(39);  // 0xB0 ~ 0xD6
        int lowPos = 161 + random.Next(93);   // 0xA1 ~ 0xFD
        byte[] bytes = [(byte)highPos, (byte)lowPos];
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding("GB2312").GetString(bytes);
    }

    protected override ClickImageCheckDefinition.ImgWrapper GetClickImg(GenerateParam param, CaptchaResource tip, SKColor color, SKBitmap bgImage)
    {
        float factor = Math.Max(bgImage.Width, bgImage.Height) / 350f;
        float fontSize = FontWrapper.DefaultFontSize * factor;
        int clickImgWidth = (int)(fontSize + 8);

        float deg = Random.Shared.Next(-30, 30);
        float fontTopCoef = fontSize * 0.1f;

        var fontImage = CaptchaImageUtils.DrawWordImage(color, tip.Data ?? "", _typeface!, fontSize,
            fontTopCoef, clickImgWidth, clickImgWidth, deg);

        return new ClickImageCheckDefinition.ImgWrapper(fontImage, tip, color);
    }
}
