using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;

namespace Tianai.Captcha.Core.Interceptor;

public class EmptyCaptchaInterceptor : ICaptchaInterceptor
{
    public static readonly EmptyCaptchaInterceptor Instance = new();
    public string Name => "empty";
}

public class CompositeCaptchaInterceptor : ICaptchaInterceptor
{
    private readonly List<ICaptchaInterceptor> _interceptors = new();

    public string Name => "composite";

    public void AddInterceptor(ICaptchaInterceptor interceptor) => _interceptors.Add(interceptor);
    public void RemoveInterceptor(ICaptchaInterceptor interceptor) => _interceptors.Remove(interceptor);
    public IReadOnlyList<ICaptchaInterceptor> Interceptors => _interceptors;

    public InterceptorContext CreateContext()
    {
        return new InterceptorContext { Name = Name, Count = _interceptors.Count, Group = this };
    }

    public ApiResponse<ImageCaptchaResponse>? BeforeGenerateCaptcha(InterceptorContext context, CaptchaType type, GenerateParam param)
    {
        foreach (var interceptor in _interceptors)
        {
            var result = interceptor.BeforeGenerateCaptcha(context, type, param);
            if (result != null) return result;
        }
        return null;
    }

    public ApiResponse<ImageCaptchaResponse>? BeforeGenerateImageCaptchaValidData(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info)
    {
        foreach (var interceptor in _interceptors)
        {
            var result = interceptor.BeforeGenerateImageCaptchaValidData(context, type, info);
            if (result != null) return result;
        }
        return null;
    }

    public void AfterGenerateImageCaptchaValidData(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info, AnyMap validData)
    {
        foreach (var interceptor in _interceptors)
            interceptor.AfterGenerateImageCaptchaValidData(context, type, info, validData);
    }

    public void AfterGenerateCaptcha(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info, ApiResponse<ImageCaptchaResponse> response)
    {
        foreach (var interceptor in _interceptors)
            interceptor.AfterGenerateCaptcha(context, type, info, response);
    }

    public ApiResponse<object>? BeforeValid(InterceptorContext context, string type, MatchParam matchParam, AnyMap validData)
    {
        foreach (var interceptor in _interceptors)
        {
            var result = interceptor.BeforeValid(context, type, matchParam, validData);
            if (result != null && !result.IsSuccess()) return result;
        }
        return ApiResponse<object>.OfSuccess();
    }

    public ApiResponse<object>? AfterValid(InterceptorContext context, string type, MatchParam matchParam, AnyMap validData, ApiResponse<object>? basicValid)
    {
        foreach (var interceptor in _interceptors)
        {
            var result = interceptor.AfterValid(context, type, matchParam, validData, basicValid);
            if (result != null && !result.IsSuccess()) return result;
        }
        return null;
    }

    public ImageCaptchaInfo? BeforeGenerateCaptchaImage(InterceptorContext context, CaptchaExchange exchange, AbstractImageCaptchaGenerator generator)
    {
        foreach (var interceptor in _interceptors)
        {
            var result = interceptor.BeforeGenerateCaptchaImage(context, exchange, generator);
            if (result != null) return result;
        }
        return null;
    }

    public void BeforeWrapImageCaptchaInfo(InterceptorContext context, CaptchaExchange exchange, AbstractImageCaptchaGenerator generator)
    {
        foreach (var interceptor in _interceptors)
            interceptor.BeforeWrapImageCaptchaInfo(context, exchange, generator);
    }

    public void AfterGenerateCaptchaImage(InterceptorContext context, CaptchaExchange exchange, ImageCaptchaInfo info, AbstractImageCaptchaGenerator generator)
    {
        foreach (var interceptor in _interceptors)
            interceptor.AfterGenerateCaptchaImage(context, exchange, info, generator);
    }
}
