using System.Collections.Concurrent;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator.Impl;

public class MultiImageCaptchaGenerator : AbstractImageCaptchaGenerator
{
    private readonly ConcurrentDictionary<string, IImageCaptchaGenerator> _generatorMap = new();
    private readonly Dictionary<string, IImageCaptchaGeneratorProvider> _providerMap = new();
    public CaptchaType DefaultCaptcha { get; set; } = CaptchaType.Slider;

    protected override void DoInit()
    {
        AddImageCaptchaGeneratorProvider(new CommonImageCaptchaGeneratorProvider(CaptchaType.Slider.ToString(),
            (rm, it, ci) => { var g = new SliderImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; }));
        AddImageCaptchaGeneratorProvider(new CommonImageCaptchaGeneratorProvider(CaptchaType.Rotate.ToString(),
            (rm, it, ci) => { var g = new RotateImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; }));
        AddImageCaptchaGeneratorProvider(new CommonImageCaptchaGeneratorProvider(CaptchaType.Concat.ToString(),
            (rm, it, ci) => { var g = new ConcatImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; }));
        AddImageCaptchaGeneratorProvider(new CommonImageCaptchaGeneratorProvider(CaptchaType.WordImageClick.ToString(),
            (rm, it, ci) => { var g = new WordClickImageCaptchaGenerator(); g.SetImageResourceManager(rm); g.SetImageTransform(it); g.SetInterceptor(ci); return g; }));
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
        var type = param.Type.ToString();
        var generator = RequireGetCaptchaGenerator(type);
        return generator.GenerateCaptchaImage(param);
    }

    public override ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type)
    {
        var typeStr = type.ToString();
        var generator = RequireGetCaptchaGenerator(typeStr);
        return generator.GenerateCaptchaImage(type);
    }

    public override ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type, string bgFormat, string tplFormat)
    {
        var typeStr = type.ToString();
        var generator = RequireGetCaptchaGenerator(typeStr);
        return generator.GenerateCaptchaImage(type, bgFormat, tplFormat);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(string type)
    {
        var generator = RequireGetCaptchaGenerator(type);
        var captchaType = Enum.TryParse<CaptchaType>(type, true, out var result) ? result : CaptchaType.Slider;
        return generator.GenerateCaptchaImage(captchaType);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(string type, string bgFormat, string tplFormat)
    {
        var generator = RequireGetCaptchaGenerator(type);
        var captchaType = Enum.TryParse<CaptchaType>(type, true, out var result) ? result : CaptchaType.Slider;
        return generator.GenerateCaptchaImage(captchaType, bgFormat, tplFormat);
    }

    public IImageCaptchaGenerator RequireGetCaptchaGenerator(string type)
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

    public void AddImageCaptchaGeneratorProvider(IImageCaptchaGeneratorProvider provider)
    {
        _providerMap[provider.Type] = provider;
    }

    public IImageCaptchaGeneratorProvider? RemoveImageCaptchaGeneratorProvider(string type)
    {
        _providerMap.Remove(type, out var provider);
        return provider;
    }

    public void AddImageCaptchaGenerator(string key, IImageCaptchaGenerator generator)
    {
        _generatorMap[key] = generator;
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
