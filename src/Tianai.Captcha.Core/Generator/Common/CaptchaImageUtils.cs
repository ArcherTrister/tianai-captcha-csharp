using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;

namespace Tianai.Captcha.Core.Generator.Common;

public static class CaptchaImageUtils
{
    public const string TypeJpg = "jpg";
    public const string TypeJpeg = "jpeg";
    public const string TypePng = "png";
    public const string TypeWebp = "webp";

    public static SKBitmap LoadImage(Stream stream)
    {
        Debug.WriteLine("开始加载图片");
        var bitmap = SKBitmap.Decode(stream);
        if (bitmap == null)
        {
            Debug.WriteLine("图片加载失败");
            throw new InvalidOperationException("Failed to decode image from stream");
        }
        Debug.WriteLine($"图片加载成功: width={bitmap.Width}, height={bitmap.Height}");
        return bitmap;
    }

    public static SKBitmap CreateTransparentImage(int width, int height)
    {
        Debug.WriteLine($"创建透明图片: width={width}, height={height}");
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);
        bitmap.Erase(SKColors.Transparent);
        Debug.WriteLine("透明图片创建完成");
        return bitmap;
    }

    public static void OverlayImage(SKBitmap baseImage, SKBitmap coverImage, int x, int y)
    {
        Debug.WriteLine($"覆盖图片: x={x}, y={y}, coverWidth={coverImage.Width}, coverHeight={coverImage.Height}");
        using var canvas = new SKCanvas(baseImage);
        canvas.DrawBitmap(coverImage, x, y);
        Debug.WriteLine("图片覆盖完成");
    }

    public static SKBitmap CutImage(SKBitmap oriImage, SKBitmap templateImage, int xPos, int yPos)
    {
        Debug.WriteLine($"开始切割图片: xPos={xPos}, yPos={yPos}, templateWidth={templateImage.Width}, templateHeight={templateImage.Height}");
        int bw = templateImage.Width;
        int bh = templateImage.Height;
        var targetImage = CreateTransparentImage(bw, bh);

        for (int y = 0; y < bh; y++)
        {
            for (int x = 0; x < bw; x++)
            {
                var templatePixel = templateImage.GetPixel(x, y);
                if (templatePixel.Alpha > 100)
                {
                    int srcX = xPos + x;
                    int srcY = yPos + y;
                    if (srcX >= 0 && srcX < oriImage.Width && srcY >= 0 && srcY < oriImage.Height)
                    {
                        targetImage.SetPixel(x, y, oriImage.GetPixel(srcX, srcY));
                    }
                }
            }
        }
        Debug.WriteLine("图片切割完成");
        return targetImage;
    }

    public static SKBitmap RotateImage(SKBitmap source, double degrees)
    {
        int w = source.Width;
        int h = source.Height;
        var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
        var rotated = new SKBitmap(info);

        using var canvas = new SKCanvas(rotated);
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(w / 2f, h / 2f);
        canvas.RotateDegrees((float)degrees);
        canvas.Translate(-w / 2f, -h / 2f);
        canvas.DrawBitmap(source, 0, 0);

        return rotated;
    }

    public static void CenterOverlayAndRotateImage(SKBitmap baseImage, SKBitmap coverImage, double degrees)
    {
        var rotated = RotateImage(coverImage, degrees);
        int bw = baseImage.Width;
        int bh = baseImage.Height;
        int cw = rotated.Width;
        int ch = rotated.Height;
        OverlayImage(baseImage, rotated, bw / 2 - cw / 2, bh / 2 - ch / 2);
        rotated.Dispose();
    }

    public static SKBitmap[] SplitImage(int pos, bool direction, SKBitmap img)
    {
        int startW, startH, endW, endH, endX, endY;

        if (direction) // horizontal split
        {
            startH = img.Height - pos;
            startW = img.Width;
            endW = img.Width;
            endH = pos;
            endX = 0;
            endY = startH;
        }
        else // vertical split
        {
            startW = pos;
            startH = img.Height;
            endW = img.Width - startW;
            endH = img.Height;
            endX = pos;
            endY = 0;
        }

        var startImg = new SKBitmap(startW, startH);
        using (var canvas = new SKCanvas(startImg))
        {
            canvas.DrawBitmap(img, SKRect.Create(0, 0, startW, startH), SKRect.Create(0, 0, startW, startH));
        }

        var endImg = new SKBitmap(endW, endH);
        using (var canvas = new SKCanvas(endImg))
        {
            canvas.DrawBitmap(img, SKRect.Create(endX, endY, endW, endH), SKRect.Create(0, 0, endW, endH));
        }

        return [startImg, endImg];
    }

    public static SKBitmap ConcatImage(bool direction, int width, int height, params SKBitmap[] imgArr)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var newImage = new SKBitmap(info);
        using var canvas = new SKCanvas(newImage);
        canvas.Clear(SKColors.Transparent);

        int pos = 0;
        foreach (var img in imgArr)
        {
            if (direction)
            {
                canvas.DrawBitmap(img, pos, 0);
                pos += img.Width;
            }
            else
            {
                canvas.DrawBitmap(img, 0, pos);
                pos += img.Height;
            }
        }
        return newImage;
    }

    public static SKBitmap DrawWordImage(SKColor fontColor, string word, SKTypeface typeface, float fontSize,
                                          float fontTopCoef, int imgWidth, int imgHeight, float degrees)
    {
        var bitmap = CreateTransparentImage(imgWidth, imgHeight);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = fontColor,
            IsAntialias = true
        };
        using var font = new SKFont(typeface, fontSize);

        float left = (imgWidth - fontSize) / 2f;
        float top = (imgHeight - fontSize) / 2f + fontSize - fontTopCoef;

        canvas.RotateDegrees(degrees, imgWidth / 2f, imgHeight / 2f);
        canvas.DrawText(word, left, top, SKTextAlign.Left, font, paint);

        return bitmap;
    }

    public static void DrawOval(int num, SKColor? color, SKCanvas canvas, int width, int height, Random random)
    {
        for (int i = 0; i < num; i++)
        {
            using var paint = new SKPaint
            {
                Color = color ?? GetRandomColor(random),
                IsStroke = true,
                IsAntialias = true
            };
            int w = 5 + random.Next(10);
            int x = random.Next(Math.Max(1, width - 25));
            int y = random.Next(Math.Max(1, height - 25));
            canvas.DrawOval(x + w / 2f, y + w / 2f, w / 2f, w / 2f, paint);
        }
    }

    public static void DrawBesselLine(int num, SKColor? color, SKCanvas canvas, int width, int height, Random random)
    {
        for (int i = 0; i < num; i++)
        {
            using var paint = new SKPaint
            {
                Color = color ?? GetRandomColor(random),
                IsStroke = true,
                StrokeWidth = 1.2f,
                IsAntialias = true
            };

            int x1 = 5, y1 = random.Next(5, Math.Max(6, height / 2));
            int x2 = width - 5, y2 = random.Next(Math.Max(1, height / 2), Math.Max(2, height - 5));
            int ctrlx = random.Next(Math.Max(1, width / 4), Math.Max(2, width * 3 / 4));
            int ctrly = random.Next(5, Math.Max(6, height - 5));

            if (random.Next(2) == 0) { (y1, y2) = (y2, y1); }

            using var path = new SKPath();
            path.MoveTo(x1, y1);
            if (random.Next(2) == 0)
            {
                path.QuadTo(ctrlx, ctrly, x2, y2);
            }
            else
            {
                int ctrlx1 = random.Next(Math.Max(1, width / 4), Math.Max(2, width * 3 / 4));
                int ctrly1 = random.Next(5, Math.Max(6, height - 5));
                path.CubicTo(ctrlx, ctrly, ctrlx1, ctrly1, x2, y2);
            }
            canvas.DrawPath(path, paint);
        }
    }

    public static SKColor GetRandomColor(Random random)
    {
        return new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
    }

    public static SKBitmap ToBufferedImage(SKBitmap image, string imageType)
    {
        if (IsJpeg(imageType))
        {
            // JPEG doesn't support alpha - draw onto white background with standard color type
            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var converted = new SKBitmap(info);
            using var canvas = new SKCanvas(converted);
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(image, 0, 0);
            return converted;
        }
        return image.Copy();
    }

    public static bool IsJpeg(string type)
        => TypeJpg.Equals(type, StringComparison.OrdinalIgnoreCase)
        || TypeJpeg.Equals(type, StringComparison.OrdinalIgnoreCase);

    public static bool IsPng(string type)
        => TypePng.Equals(type, StringComparison.OrdinalIgnoreCase);

    public static SKEncodedImageFormat GetImageFormat(string formatName)
    {
        return formatName.ToLowerInvariant() switch
        {
            "webp" => SKEncodedImageFormat.Webp,
            "png" => SKEncodedImageFormat.Png,
            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
            _ => SKEncodedImageFormat.Jpeg
        };
    }

    public static void DrawWatermark(SKBitmap bgImage, string watermark, int x, int y, SKColor color, SKTypeface typeface, float fontSize)
    {
        if (bgImage == null || string.IsNullOrEmpty(watermark)) return;

        using var canvas = new SKCanvas(bgImage);
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };
        using var font = new SKFont(typeface, fontSize);

        float textWidth = font.MeasureText(watermark);
        float textHeight = fontSize;

        int spacingX = (int)textWidth + 50;
        int spacingY = (int)textHeight + 50;
        int imageWidth = bgImage.Width;
        int imageHeight = bgImage.Height;
        int rows = (int)Math.Ceiling((double)imageHeight / spacingY) + 2;
        int cols = (int)Math.Ceiling((double)imageWidth / spacingX) + 2;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int currentX = col * spacingX + x;
                int currentY = row * spacingY + y;

                canvas.Save();
                canvas.Translate(currentX, currentY);
                canvas.RotateDegrees(45);
                canvas.DrawText(watermark, 0, fontSize, SKTextAlign.Left, font, paint);
                canvas.Restore();
            }
        }
    }
}
