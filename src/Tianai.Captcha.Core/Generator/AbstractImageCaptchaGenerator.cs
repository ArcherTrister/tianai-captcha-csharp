using SkiaSharp;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator.Common;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator;

public abstract class AbstractImageCaptchaGenerator : IImageCaptchaGenerator
{
    private IImageCaptchaResourceManager _resourceManager = null!;
    private IImageTransform? _imageTransform;
    private ICaptchaInterceptor? _interceptor;
    private bool _initialized;
    protected readonly Random Rng = Random.Shared;

    public IImageCaptchaGenerator Init()
    {
        if (_imageTransform == null)
            _imageTransform = new Impl.Base64ImageTransform();
        DoInit();
        _initialized = true;
        return this;
    }

    protected abstract void DoInit();
    protected abstract void DoGenerateCaptchaImage(CaptchaExchange exchange);
    protected abstract ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange);

    public virtual ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type)
    {
        return GenerateCaptchaImage(GenerateParam.CreateBuilder().Type(type).Build());
    }

    public virtual ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type, string bgFormat, string templateFormat)
    {
        return GenerateCaptchaImage(GenerateParam.CreateBuilder().Type(type)
            .BackgroundFormatName(bgFormat).TemplateFormatName(templateFormat).Build());
    }

    public virtual ImageCaptchaInfo GenerateCaptchaImage(GenerateParam param)
    {
        AssertInit();
        var data = new CustomData();
        var exchange = CaptchaExchange.Create(data, param);

        // Before generate hook
        var interceptorResult = BeforeGenerate(exchange);
        if (interceptorResult != null) return interceptorResult;

        DoGenerateCaptchaImage(exchange);
        BeforeWrapImageCaptchaInfo(exchange);
        var info = WrapImageCaptchaInfo(exchange);
        AfterGenerateCaptchaImage(exchange, info);
        return info;
    }

    protected virtual ImageCaptchaInfo? BeforeGenerate(CaptchaExchange exchange)
    {
        var context = _interceptor?.CreateContext() ?? new InterceptorContext();
        return _interceptor?.BeforeGenerateCaptchaImage(context, exchange, this);
    }

    protected virtual void BeforeWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var context = _interceptor?.CreateContext() ?? new InterceptorContext();
        _interceptor?.BeforeWrapImageCaptchaInfo(context, exchange, this);
    }

    protected virtual void AfterGenerateCaptchaImage(CaptchaExchange exchange, ImageCaptchaInfo info)
    {
        var context = _interceptor?.CreateContext() ?? new InterceptorContext();
        _interceptor?.AfterGenerateCaptchaImage(context, exchange, info, this);
    }

    protected ImageCaptchaInfo WrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        var info = DoWrapImageCaptchaInfo(exchange);
        info.Data = exchange.CustomData;
        // 设置自定义容错值 (对应 Java ParamKeyEnum.TOLERANT)
        if (exchange.Param.ContainsKey(ParamKeyEnum.Tolerant.Key))
        {
            var tolerant = exchange.Param.GetParam(ParamKeyEnum.Tolerant);
            info.Tolerant = tolerant;
        }
        return info;
    }

    // Helpers
    protected ResourceMap RequiredRandomGetTemplate(CaptchaType type, string? tag)
    {
        return _resourceManager.RandomGetTemplate(type, tag);
    }

    protected CaptchaResource RequiredRandomGetResource(CaptchaType type, string? tag)
    {
        return _resourceManager.RandomGetResource(type, tag);
    }

    protected SKBitmap GetTemplateImage(ResourceMap templateImages, string imageName)
    {
        var resource = templateImages.Get(imageName)
            ?? throw new ImageCaptchaException($"Template image not found: {imageName}");
        using var stream = _resourceManager.GetResourceStream(resource);
        return CaptchaImageUtils.LoadImage(stream);
    }

    protected SKBitmap? GetTemplateImageOrNull(ResourceMap templateImages, string imageName)
    {
        var resource = templateImages.Get(imageName);
        if (resource == null) return null;
        using var stream = _resourceManager.GetResourceStream(resource);
        return CaptchaImageUtils.LoadImage(stream);
    }

    protected SKBitmap GetResourceImage(CaptchaResource resource)
    {
        using var stream = _resourceManager.GetResourceStream(resource);
        return CaptchaImageUtils.LoadImage(stream);
    }

    protected int RandomInt(int origin, int bound)
    {
        if (origin >= bound) return origin;
        return Rng.Next(origin, bound);
    }

    protected int RandomInt(int bound) => Rng.Next(bound);

    protected bool RandomBoolean() => Rng.Next(2) == 0;

    private void AssertInit()
    {
        if (!_initialized)
            throw new ImageCaptchaException("Generator not initialized. Call Init() first.");
    }

    // Property accessors
    public IImageCaptchaResourceManager GetImageResourceManager() => _resourceManager;
    public void SetImageResourceManager(IImageCaptchaResourceManager manager) => _resourceManager = manager;
    public IImageTransform? GetImageTransform() => _imageTransform;
    public void SetImageTransform(IImageTransform transform) => _imageTransform = transform;
    public ICaptchaInterceptor? GetInterceptor() => _interceptor;
    public void SetInterceptor(ICaptchaInterceptor interceptor) => _interceptor = interceptor;
}
