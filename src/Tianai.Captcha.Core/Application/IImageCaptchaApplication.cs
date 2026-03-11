using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Validator;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Application;

public interface IImageCaptchaApplication : IDisposable
{
    ApiResponse<ImageCaptchaResponse> GenerateCaptcha();
    ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type);
    ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaImageType captchaImageType);
    ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type, CaptchaImageType captchaImageType);
    ApiResponse<ImageCaptchaResponse> GenerateCaptcha(GenerateParam param);
    ApiResponse<object> Matching(string id, MatchParam matchParam);
    ApiResponse<object> Matching(string id, ImageCaptchaTrack track);
    bool Matching(string id, float percentage);
    string? GetCaptchaTypeById(string id);
    ApiResponse<object> VerifySecondaryToken(string token);
    IImageCaptchaResourceManager GetImageCaptchaResourceManager();
    IImageCaptchaValidator GetImageCaptchaValidator();
    IImageCaptchaGenerator GetImageCaptchaGenerator();
    ICacheStore GetCacheStore();
    ICaptchaInterceptor GetCaptchaInterceptor();
}
