using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator;

public interface IImageTransform
{
    ImageTransformData Transform(GenerateParam param, SKBitmap? backgroundImage, SKBitmap? templateImage,
                                  CaptchaResource? backgroundResource, ResourceMap? templateResource,
                                  CustomData data);
}

public interface IImageCaptchaGenerator
{
    IImageCaptchaGenerator Init();
    ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type);
    ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type, string targetFormatName, string matrixFormatName);
    ImageCaptchaInfo GenerateCaptchaImage(GenerateParam param);
    IImageCaptchaResourceManager GetImageResourceManager();
    void SetImageResourceManager(IImageCaptchaResourceManager manager);
    IImageTransform? GetImageTransform();
    void SetImageTransform(IImageTransform transform);
    ICaptchaInterceptor? GetInterceptor();
    void SetInterceptor(ICaptchaInterceptor interceptor);
}

public interface IImageCaptchaGeneratorProvider
{
    string Type { get; }
    IImageCaptchaGenerator Get(IImageCaptchaResourceManager resourceManager, IImageTransform imageTransform, ICaptchaInterceptor interceptor);
}
