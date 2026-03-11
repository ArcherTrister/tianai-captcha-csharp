using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;

namespace Tianai.Captcha.Core.Interceptor;

public class InterceptorContext
{
    public string Name { get; set; } = "interceptor";
    public InterceptorContext? Parent { get; set; }
    public int Current { get; set; }
    public int Count { get; set; }
    public ICaptchaInterceptor? Group { get; set; }
    public object? PreReturnData { get; set; }
    public AnyMap Data { get; set; } = new();

    public void PutData(string key, object? value) => Data[key] = value;
    public T? GetData<T>(string key) => Data.TryGetValue(key, out var val) && val is T typed ? typed : default;
    public int Next() => ++Current;
    public int End() { Current = Count; return Current; }
    public bool IsEnd => Current >= Count;
    public bool IsStart => Current == 0;
}

public interface ICaptchaInterceptor
{
    string Name => "interceptor";

    InterceptorContext CreateContext() => new() { Name = Name };

    // Generate captcha lifecycle
    ApiResponse<ImageCaptchaResponse>? BeforeGenerateCaptcha(InterceptorContext context, CaptchaType type, GenerateParam param) => null;
    ApiResponse<ImageCaptchaResponse>? BeforeGenerateImageCaptchaValidData(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info) => null;
    void AfterGenerateImageCaptchaValidData(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info, AnyMap validData) { }
    void AfterGenerateCaptcha(InterceptorContext context, CaptchaType type, ImageCaptchaInfo info, ApiResponse<ImageCaptchaResponse> response) { }

    // Validate captcha lifecycle
    ApiResponse<object>? BeforeValid(InterceptorContext context, string type, MatchParam matchParam, AnyMap validData) => ApiResponse<object>.OfSuccess();
    ApiResponse<object>? AfterValid(InterceptorContext context, string type, MatchParam matchParam, AnyMap validData, ApiResponse<object>? basicValid) => null;

    // Low-level generator hooks
    ImageCaptchaInfo? BeforeGenerateCaptchaImage(InterceptorContext context, CaptchaExchange exchange, AbstractImageCaptchaGenerator generator) => null;
    void BeforeWrapImageCaptchaInfo(InterceptorContext context, CaptchaExchange exchange, AbstractImageCaptchaGenerator generator) { }
    void AfterGenerateCaptchaImage(InterceptorContext context, CaptchaExchange exchange, ImageCaptchaInfo info, AbstractImageCaptchaGenerator generator) { }
}
