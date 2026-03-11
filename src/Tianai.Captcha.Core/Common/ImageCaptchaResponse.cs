namespace Tianai.Captcha.Core.Common;

public class ImageCaptchaResponse
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? BackgroundImage { get; set; }
    public string? TemplateImage { get; set; }
    public string? BackgroundImageTag { get; set; }
    public string? TemplateImageTag { get; set; }
    public int BackgroundImageWidth { get; set; }
    public int BackgroundImageHeight { get; set; }
    public int TemplateImageWidth { get; set; }
    public int TemplateImageHeight { get; set; }
    public object? Data { get; set; }
}
