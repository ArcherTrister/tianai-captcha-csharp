using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Validator;

public interface IImageCaptchaValidator
{
    AnyMap GenerateImageCaptchaValidData(ImageCaptchaInfo imageCaptchaInfo);
    ApiResponse<object> Valid(ImageCaptchaTrack track, AnyMap validData);
}
