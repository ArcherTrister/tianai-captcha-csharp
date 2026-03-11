namespace Tianai.Captcha.Core.Common;

public class CustomData
{
    public AnyMap ViewData { get; set; } = new();
    public AnyMap Data { get; set; } = new();
    public object? Expand { get; set; }
}
