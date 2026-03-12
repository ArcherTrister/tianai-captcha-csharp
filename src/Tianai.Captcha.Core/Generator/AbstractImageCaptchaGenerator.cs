using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    protected readonly ILogger _logger;

    protected AbstractImageCaptchaGenerator()
    {
        
    }

    protected AbstractImageCaptchaGenerator(ILogger logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    public IImageCaptchaGenerator Init()
    {
        _logger.LogDebug("开始初始化验证码生成器");
        if (_imageTransform == null)
        {
            _logger.LogDebug("设置默认图片转换器");
            _imageTransform = new Impl.Base64ImageTransform();
        }
        DoInit();
        _initialized = true;
        _logger.LogDebug("验证码生成器初始化完成");
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
        _logger.LogDebug("开始生成验证码图片: type={Type}", param.CaptchaType);
        AssertInit();
        var data = new CustomData();
        var exchange = CaptchaExchange.Create(data, param);

        // Before generate hook
        _logger.LogDebug("执行验证码生成前钩子");
        var interceptorResult = BeforeGenerate(exchange);
        if (interceptorResult != null)
        {
            _logger.LogDebug("验证码生成前钩子返回结果");
            return interceptorResult;
        }

        _logger.LogDebug("执行验证码图片生成");
        DoGenerateCaptchaImage(exchange);
        _logger.LogDebug("执行验证码信息包装前钩子");
        BeforeWrapImageCaptchaInfo(exchange);
        _logger.LogDebug("包装验证码信息");
        var info = WrapImageCaptchaInfo(exchange);
        _logger.LogDebug("执行验证码生成后钩子");
        AfterGenerateCaptchaImage(exchange, info);
        _logger.LogDebug("验证码图片生成完成: type={Type}", param.CaptchaType);
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
        _logger.LogDebug("获取模板资源: type={Type}, tag={Tag}", type, tag);
        return _resourceManager.RandomGetTemplate(type, tag);
    }

    protected CaptchaResource RequiredRandomGetResource(CaptchaType type, string? tag)
    {
        _logger.LogDebug("获取背景资源: type={Type}, tag={Tag}", type, tag);
        return _resourceManager.RandomGetResource(type, tag);
    }

    protected SKBitmap GetTemplateImage(ResourceMap templateImages, string imageName)
    {
        _logger.LogDebug("获取模板图片: {ImageName}", imageName);
        var resource = templateImages.Get(imageName)
            ?? throw new ImageCaptchaException($"Template image not found: {imageName}");
        using var stream = _resourceManager.GetResourceStream(resource);
        var bitmap = CaptchaImageUtils.LoadImage(stream);
        _logger.LogDebug("模板图片加载成功: {ImageName}, width={Width}, height={Height}", imageName, bitmap.Width, bitmap.Height);
        return bitmap;
    }

    protected SKBitmap? GetTemplateImageOrNull(ResourceMap templateImages, string imageName)
    {
        _logger.LogDebug("尝试获取模板图片: {ImageName}", imageName);
        var resource = templateImages.Get(imageName);
        if (resource == null)
        {
            _logger.LogDebug("模板图片不存在: {ImageName}", imageName);
            return null;
        }
        using var stream = _resourceManager.GetResourceStream(resource);
        var bitmap = CaptchaImageUtils.LoadImage(stream);
        _logger.LogDebug("模板图片加载成功: {ImageName}, width={Width}, height={Height}", imageName, bitmap.Width, bitmap.Height);
        return bitmap;
    }

    protected SKBitmap GetResourceImage(CaptchaResource resource)
    {
        _logger.LogDebug("获取背景图片: data={Data}", resource.Data);
        using var stream = _resourceManager.GetResourceStream(resource);
        var bitmap = CaptchaImageUtils.LoadImage(stream);
        _logger.LogDebug("背景图片加载成功: width={Width}, height={Height}", bitmap.Width, bitmap.Height);
        return bitmap;
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
