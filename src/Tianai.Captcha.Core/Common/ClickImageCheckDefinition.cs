using SkiaSharp;

namespace Tianai.Captcha.Core.Common;

public class ClickImageCheckDefinition
{
    public CaptchaResource? Tip { get; set; }
    public ImgWrapper? TipImage { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public SKColor ImageColor { get; set; }

    public class ImgWrapper
    {
        public SKBitmap? Image { get; set; }
        public CaptchaResource? Tip { get; set; }
        public SKColor ImageColor { get; set; }

        public ImgWrapper() { }

        public ImgWrapper(SKBitmap? image, CaptchaResource? tip, SKColor imageColor)
        {
            Image = image;
            Tip = tip;
            ImageColor = imageColor;
        }
    }
}
