using SkiaSharp;
using Tianai.Captcha.Core.Generator.Common;

namespace Tianai.Captcha.Core.Tests.Generator;

public class CaptchaImageUtilsTests
{
    [Fact]
    public void CreateTransparentImage_HasCorrectDimensions()
    {
        using var bmp = CaptchaImageUtils.CreateTransparentImage(100, 50);
        Assert.Equal(100, bmp.Width);
        Assert.Equal(50, bmp.Height);
    }

    [Fact]
    public void OverlayImage_DrawsCoverOnBase()
    {
        using var baseImg = CaptchaImageUtils.CreateTransparentImage(100, 100);
        using var cover = CaptchaImageUtils.CreateTransparentImage(10, 10);

        // Fill cover with red
        using (var canvas = new SKCanvas(cover))
        {
            canvas.Clear(SKColors.Red);
        }

        CaptchaImageUtils.OverlayImage(baseImg, cover, 5, 5);
        var pixel = baseImg.GetPixel(5, 5);
        Assert.Equal(SKColors.Red, pixel);
    }

    [Fact]
    public void RotateImage_HasSameDimensions()
    {
        using var source = CaptchaImageUtils.CreateTransparentImage(80, 80);
        using (var canvas = new SKCanvas(source))
        {
            canvas.Clear(SKColors.Blue);
        }

        using var rotated = CaptchaImageUtils.RotateImage(source, 45);
        Assert.Equal(80, rotated.Width);
        Assert.Equal(80, rotated.Height);
    }

    [Fact]
    public void CutImage_ExtractsPixelsWhereAlphaHigh()
    {
        using var ori = CaptchaImageUtils.CreateTransparentImage(100, 100);
        using (var canvas = new SKCanvas(ori))
        {
            canvas.Clear(SKColors.Green);
        }

        // Template with opaque center
        using var template = CaptchaImageUtils.CreateTransparentImage(20, 20);
        using (var canvas = new SKCanvas(template))
        {
            canvas.Clear(SKColors.White); // alpha = 255 > 100
        }

        using var cut = CaptchaImageUtils.CutImage(ori, template, 10, 10);
        Assert.Equal(20, cut.Width);
        Assert.Equal(20, cut.Height);

        // The cut pixel should be green (from original)
        var pixel = cut.GetPixel(5, 5);
        Assert.Equal(SKColors.Green.Red, pixel.Red);
        Assert.Equal(SKColors.Green.Green, pixel.Green);
    }

    [Fact]
    public void SplitImage_Horizontal_ReturnsTwoParts()
    {
        using var img = CaptchaImageUtils.CreateTransparentImage(100, 100);
        using (var canvas = new SKCanvas(img))
        {
            canvas.Clear(SKColors.Yellow);
        }

        var parts = CaptchaImageUtils.SplitImage(30, true, img);
        try
        {
            Assert.Equal(2, parts.Length);
            Assert.Equal(100, parts[0].Width);
            Assert.Equal(70, parts[0].Height); // height - pos
            Assert.Equal(100, parts[1].Width);
            Assert.Equal(30, parts[1].Height); // pos
        }
        finally
        {
            foreach (var p in parts) p.Dispose();
        }
    }

    [Fact]
    public void SplitImage_Vertical_ReturnsTwoParts()
    {
        using var img = CaptchaImageUtils.CreateTransparentImage(100, 100);
        using (var canvas = new SKCanvas(img))
        {
            canvas.Clear(SKColors.Cyan);
        }

        var parts = CaptchaImageUtils.SplitImage(40, false, img);
        try
        {
            Assert.Equal(2, parts.Length);
            Assert.Equal(40, parts[0].Width);
            Assert.Equal(100, parts[0].Height);
            Assert.Equal(60, parts[1].Width);
            Assert.Equal(100, parts[1].Height);
        }
        finally
        {
            foreach (var p in parts) p.Dispose();
        }
    }

    [Fact]
    public void ConcatImage_Horizontal_HasCorrectSize()
    {
        using var img1 = CaptchaImageUtils.CreateTransparentImage(50, 100);
        using var img2 = CaptchaImageUtils.CreateTransparentImage(50, 100);

        using var result = CaptchaImageUtils.ConcatImage(true, 100, 100, img1, img2);
        Assert.Equal(100, result.Width);
        Assert.Equal(100, result.Height);
    }

    [Fact]
    public void GetImageFormat_ReturnsCorrectFormats()
    {
        Assert.Equal(SKEncodedImageFormat.Jpeg, CaptchaImageUtils.GetImageFormat("jpg"));
        Assert.Equal(SKEncodedImageFormat.Jpeg, CaptchaImageUtils.GetImageFormat("jpeg"));
        Assert.Equal(SKEncodedImageFormat.Png, CaptchaImageUtils.GetImageFormat("png"));
        Assert.Equal(SKEncodedImageFormat.Webp, CaptchaImageUtils.GetImageFormat("webp"));
        Assert.Equal(SKEncodedImageFormat.Jpeg, CaptchaImageUtils.GetImageFormat("unknown"));
    }

    [Fact]
    public void IsJpeg_DetectsJpegFormats()
    {
        Assert.True(CaptchaImageUtils.IsJpeg("jpg"));
        Assert.True(CaptchaImageUtils.IsJpeg("jpeg"));
        Assert.True(CaptchaImageUtils.IsJpeg("JPG"));
        Assert.False(CaptchaImageUtils.IsJpeg("png"));
    }

    [Fact]
    public void IsPng_DetectsPngFormat()
    {
        Assert.True(CaptchaImageUtils.IsPng("png"));
        Assert.True(CaptchaImageUtils.IsPng("PNG"));
        Assert.False(CaptchaImageUtils.IsPng("jpg"));
    }

    [Fact]
    public void GetRandomColor_ReturnsValidColor()
    {
        var random = new Random(42);
        var color = CaptchaImageUtils.GetRandomColor(random);
        Assert.Equal(255, color.Alpha); // fully opaque
    }

    [Fact]
    public void ToBufferedImage_JpegType_ConvertsToOpaque()
    {
        using var src = CaptchaImageUtils.CreateTransparentImage(50, 50);
        using var result = CaptchaImageUtils.ToBufferedImage(src, "jpg");
        Assert.Equal(50, result.Width);
        Assert.Equal(50, result.Height);
    }

    [Fact]
    public void ToBufferedImage_PngType_ReturnsCopy()
    {
        using var src = CaptchaImageUtils.CreateTransparentImage(50, 50);
        using var result = CaptchaImageUtils.ToBufferedImage(src, "png");
        Assert.Equal(50, result.Width);
        Assert.Equal(50, result.Height);
    }

    [Fact]
    public void DrawWordImage_CreatesNonEmptyBitmap()
    {
        using var typeface = SKTypeface.Default;
        using var bmp = CaptchaImageUtils.DrawWordImage(
            SKColors.Black, "A", typeface, 24f, 0f, 40, 40, 0);

        Assert.Equal(40, bmp.Width);
        Assert.Equal(40, bmp.Height);
    }
}
