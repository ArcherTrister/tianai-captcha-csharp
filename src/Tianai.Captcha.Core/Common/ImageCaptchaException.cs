namespace Tianai.Captcha.Core.Common;

public class ImageCaptchaException : Exception
{
    public ImageCaptchaException(string message) : base(message) { }
    public ImageCaptchaException(string message, Exception innerException) : base(message, innerException) { }
}
