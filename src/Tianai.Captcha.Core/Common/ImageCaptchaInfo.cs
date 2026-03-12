namespace Tianai.Captcha.Core.Common;

public class ImageCaptchaInfo
{
    public string? BackgroundImage { get; set; }
    public string? TemplateImage { get; set; }
    public string? BackgroundImageTag { get; set; }
    public string? TemplateImageTag { get; set; }
    public int BackgroundImageWidth { get; set; }
    public int BackgroundImageHeight { get; set; }
    public int TemplateImageWidth { get; set; }
    public int TemplateImageHeight { get; set; }
    public int? RandomX { get; set; }
    public float? Tolerant { get; set; }
    public CaptchaType Type { get; set; } = CaptchaType.Slider;
    public CustomData? Data { get; set; }

    public ImageCaptchaInfo() { }

    public ImageCaptchaInfo(
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        int? randomX, CaptchaType type)
    {
        BackgroundImage = backgroundImage;
        TemplateImage = templateImage;
        BackgroundImageTag = backgroundImageTag;
        TemplateImageTag = templateImageTag;
        BackgroundImageWidth = backgroundImageWidth;
        BackgroundImageHeight = backgroundImageHeight;
        TemplateImageWidth = templateImageWidth;
        TemplateImageHeight = templateImageHeight;
        RandomX = randomX;
        Type = type;
    }

    public ImageCaptchaInfo(
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        int? randomX, string type)
    {
        BackgroundImage = backgroundImage;
        TemplateImage = templateImage;
        BackgroundImageTag = backgroundImageTag;
        TemplateImageTag = templateImageTag;
        BackgroundImageWidth = backgroundImageWidth;
        BackgroundImageHeight = backgroundImageHeight;
        TemplateImageWidth = templateImageWidth;
        TemplateImageHeight = templateImageHeight;
        RandomX = randomX;
        Type = Enum.Parse<CaptchaType>(type);
    }
}

public class SliderImageCaptchaInfo : ImageCaptchaInfo
{
    public int X { get; set; }
    public int Y { get; set; }

    public SliderImageCaptchaInfo() { }

    public static SliderImageCaptchaInfo Of(
        int x, int y,
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        CaptchaType type, CustomData? data = null, float? tolerant = null)
    {
        return new SliderImageCaptchaInfo
        {
            X = x, Y = y,
            BackgroundImage = backgroundImage,
            TemplateImage = templateImage,
            BackgroundImageTag = backgroundImageTag,
            TemplateImageTag = templateImageTag,
            BackgroundImageWidth = backgroundImageWidth,
            BackgroundImageHeight = backgroundImageHeight,
            TemplateImageWidth = templateImageWidth,
            TemplateImageHeight = templateImageHeight,
            RandomX = x,
            Type = type,
            Data = data,
            Tolerant = tolerant
        };
    }

    public static SliderImageCaptchaInfo Of(
        int x, int y,
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        string type, CustomData? data = null, float? tolerant = null)
    {
        return new SliderImageCaptchaInfo
        {
            X = x, Y = y,
            BackgroundImage = backgroundImage,
            TemplateImage = templateImage,
            BackgroundImageTag = backgroundImageTag,
            TemplateImageTag = templateImageTag,
            BackgroundImageWidth = backgroundImageWidth,
            BackgroundImageHeight = backgroundImageHeight,
            TemplateImageWidth = templateImageWidth,
            TemplateImageHeight = templateImageHeight,
            RandomX = x,
            Type = Enum.Parse<CaptchaType>(type),
            Data = data,
            Tolerant = tolerant
        };
    }
}

public class RotateImageCaptchaInfo : ImageCaptchaInfo
{
    public double Degree { get; set; }
    public static readonly float DefaultTolerant = 0.005f;

    public RotateImageCaptchaInfo() { }

    public static RotateImageCaptchaInfo Of(
        double degree,
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        int? randomX, CaptchaType type, CustomData? data = null, float? tolerant = null)
    {
        return new RotateImageCaptchaInfo
        {
            Degree = degree,
            BackgroundImage = backgroundImage,
            TemplateImage = templateImage,
            BackgroundImageTag = backgroundImageTag,
            TemplateImageTag = templateImageTag,
            BackgroundImageWidth = backgroundImageWidth,
            BackgroundImageHeight = backgroundImageHeight,
            TemplateImageWidth = templateImageWidth,
            TemplateImageHeight = templateImageHeight,
            RandomX = randomX,
            Type = type,
            Data = data,
            Tolerant = tolerant ?? DefaultTolerant
        };
    }

    public static RotateImageCaptchaInfo Of(
        double degree,
        string? backgroundImage, string? templateImage,
        string? backgroundImageTag, string? templateImageTag,
        int backgroundImageWidth, int backgroundImageHeight,
        int templateImageWidth, int templateImageHeight,
        int? randomX, string type, CustomData? data = null, float? tolerant = null)
    {
        return new RotateImageCaptchaInfo
        {
            Degree = degree,
            BackgroundImage = backgroundImage,
            TemplateImage = templateImage,
            BackgroundImageTag = backgroundImageTag,
            TemplateImageTag = templateImageTag,
            BackgroundImageWidth = backgroundImageWidth,
            BackgroundImageHeight = backgroundImageHeight,
            TemplateImageWidth = templateImageWidth,
            TemplateImageHeight = templateImageHeight,
            RandomX = randomX,
            Type = Enum.Parse<CaptchaType>(type),
            Data = data,
            Tolerant = tolerant ?? DefaultTolerant
        };
    }
}
