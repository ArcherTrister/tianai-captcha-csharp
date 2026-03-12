using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator.Impl;

public class MultiImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    private readonly ConcurrentDictionary<CaptchaType, IImageCaptchaGenerator> _generatorMap = new();
    private readonly Dictionary<CaptchaType, IImageCaptchaGeneratorProvider> _providerMap = new();
    public CaptchaType DefaultCaptcha { get; set; } = CaptchaType.Slider;

    public MultiImageCaptchaGenerator() : base(null)
    {
        
    }

    public MultiImageCaptchaGenerator(ILogger logger) : base(logger)
    {
    }

    protected override void DoInit()
    {
        AddImageCaptchaGeneratorProvider(CaptchaType.Slider,
            (rm, it, ci) => { var g = new SliderImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; });
        AddImageCaptchaGeneratorProvider(CaptchaType.Rotate,
            (rm, it, ci) => { var g = new RotateImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; });
        AddImageCaptchaGeneratorProvider(CaptchaType.Concat,
            (rm, it, ci) => { var g = new ConcatImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; });
        AddImageCaptchaGeneratorProvider(CaptchaType.WordImageClick,
            (rm, it, ci) => { var g = new WordClickImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; });
    }

    protected override void DoGenerateCaptchaImage(CaptchaExchange exchange)
    {
        // Not used directly - routing happens in GenerateCaptchaImage override
    }

    protected override ImageCaptchaInfo DoWrapImageCaptchaInfo(CaptchaExchange exchange)
    {
        // Not used directly
        throw new NotSupportedException();
    }

    public override ImageCaptchaInfo GenerateCaptchaImage(GenerateParam param)
    {
        var type = param.CaptchaType;
        _logger.LogInformation($"MultiImageCaptchaGenerator: Generating captcha of type: {type}");
        var generator = RequireGetCaptchaGenerator(type);
        _logger.LogInformation($"MultiImageCaptchaGenerator: Using generator: {generator.GetType().Name}");
        var result = generator.GenerateCaptchaImage(param);
        _logger.LogInformation($"MultiImageCaptchaGenerator: Generated captcha of type: {result.Type}");
        return result;
    }

    public override ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type)
    {
        var generator = RequireGetCaptchaGenerator(type);
        return generator.GenerateCaptchaImage(type);
    }

    public override ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type, string bgFormat, string tplFormat)
    {
        var generator = RequireGetCaptchaGenerator(type);
        return generator.GenerateCaptchaImage(type, bgFormat, tplFormat);
    }

    public IImageCaptchaGenerator RequireGetCaptchaGenerator(CaptchaType type)
    {
        return _generatorMap.GetOrAdd(type, t =>
        {
            if (!_providerMap.TryGetValue(t, out var provider))
                throw new ImageCaptchaException($"No generator provider found for type: {t}");

            var generator = provider.Get(GetImageResourceManager(), GetImageTransform()!, GetInterceptor()!);
            generator.Init();
            return generator;
        });
    }

    public void AddImageCaptchaGeneratorProvider(CaptchaType type, Func<IImageCaptchaResourceManager, IImageTransform, ICaptchaInterceptor, IImageCaptchaGenerator> factory)
    {
        _providerMap[type] = new CommonImageCaptchaGeneratorProvider(type.ToString(), factory);
    }

    public void AddImageCaptchaGeneratorProvider(IImageCaptchaGeneratorProvider provider)
    {
        if (Enum.TryParse<CaptchaType>(provider.Type, true, out var captchaType))
        {
            _providerMap[captchaType] = provider;
        }
    }

    public IImageCaptchaGeneratorProvider? RemoveImageCaptchaGeneratorProvider(CaptchaType type)
    {
        _providerMap.Remove(type, out var provider);
        return provider;
    }

    public void AddImageCaptchaGenerator(CaptchaType type, IImageCaptchaGenerator generator)
    {
        _generatorMap[type] = generator;
    }
}

public class CommonImageCaptchaGeneratorProvider : IImageCaptchaGeneratorProvider
{
    public string Type { get; }
    private readonly Func<IImageCaptchaResourceManager, IImageTransform, ICaptchaInterceptor, IImageCaptchaGenerator> _factory;

    public CommonImageCaptchaGeneratorProvider(string type,
        Func<IImageCaptchaResourceManager, IImageTransform, ICaptchaInterceptor, IImageCaptchaGenerator> factory)
    {
        Type = type;
        _factory = factory;
    }

    public IImageCaptchaGenerator Get(IImageCaptchaResourceManager resourceManager, IImageTransform imageTransform, ICaptchaInterceptor interceptor)
    {
        return _factory(resourceManager, imageTransform, interceptor);
    }
}
