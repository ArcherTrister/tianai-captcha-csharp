namespace Tianai.Captcha.Core.Common;

public class CaptchaResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? Type { get; set; }
    public string? Data { get; set; }
    public string? Tag { get; set; }
    public string? Tip { get; set; }
    public object? Extra { get; set; }

    public CaptchaResource() { }

    public CaptchaResource(string? type, string? data)
    {
        Type = type;
        Data = data;
    }

    public CaptchaResource(string? type, string? data, string? tag)
    {
        Type = type;
        Data = data;
        Tag = tag;
    }
}
