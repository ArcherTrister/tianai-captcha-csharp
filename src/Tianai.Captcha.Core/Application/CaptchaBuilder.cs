using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Cache.Impl;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;
using Tianai.Captcha.Core.Generator.Impl;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;
using Tianai.Captcha.Core.Resource.Impl;
using Tianai.Captcha.Core.Validator;
using Tianai.Captcha.Core.Validator.Impl;

namespace Tianai.Captcha.Core.Application;

public class CaptchaBuilder
{
    private IResourceStore? _resourceStore;
    private ICacheStore? _cacheStore;
    private IImageCaptchaGenerator? _generator;
    private IImageCaptchaValidator? _validator;
    private ICaptchaInterceptor? _interceptor;
    private IImageTransform? _imageTransform;
    private ImageCaptchaOptions _options = new();
    //private bool _addDefaultTemplate;
    private readonly List<CaptchaResource> _fonts = new();
    private readonly List<(CaptchaType type, CaptchaResource resource)> _resources = new();
    private readonly List<(CaptchaType type, ResourceMap template)> _templates = new();

    public static CaptchaBuilder Create() => new();

    public CaptchaBuilder SetResourceStore(IResourceStore store) { _resourceStore = store; return this; }
    public CaptchaBuilder SetCacheStore(ICacheStore store) { _cacheStore = store; return this; }
    public CaptchaBuilder SetGenerator(IImageCaptchaGenerator generator) { _generator = generator; return this; }
    public CaptchaBuilder SetValidator(IImageCaptchaValidator validator) { _validator = validator; return this; }
    public CaptchaBuilder SetInterceptor(ICaptchaInterceptor interceptor) { _interceptor = interceptor; return this; }
    public CaptchaBuilder SetImageTransform(IImageTransform transform) { _imageTransform = transform; return this; }
    public CaptchaBuilder SetOptions(ImageCaptchaOptions options) { _options = options; return this; }

    // public CaptchaBuilder AddDefaultTemplate()
    // {
    //     _addDefaultTemplate = true;
    //
    //     return this;
    // }

    public CaptchaBuilder AddFont(CaptchaResource fontResource) { _fonts.Add(fontResource); return this; }

    public CaptchaBuilder AddBackgroundResource(CaptchaType captchaType, CaptchaResource resource)
    {
        _resources.Add((captchaType, resource));
        return this;
    }

    public CaptchaBuilder AddTemplate(CaptchaType captchaType, ResourceMap template)
    {
        _templates.Add((captchaType, template));
        return this;
    }

    public CaptchaBuilder Cached(int size = 10, int waitTime = 1000, int period = 5000,
        long? expireTime = null, HashSet<string>? ignoredCacheFields = null)
    {
        _options.LocalCacheEnabled = true;
        _options.LocalCacheSize = size;
        _options.LocalCacheWaitTime = waitTime;
        _options.LocalCachePeriod = period;
        _options.LocalCacheExpireTime = expireTime;
        _options.LocalCacheIgnoredCacheFields = ignoredCacheFields;
        return this;
    }

    public CaptchaBuilder Prefix(string prefix) { _options.Prefix = prefix; return this; }

    public CaptchaBuilder DefaultTolerant(float tolerant) { _options.DefaultTolerant = tolerant; return this; }

    public CaptchaBuilder SliderTolerant(float tolerant) { _options.SliderTolerant = tolerant; return this; }

    public CaptchaBuilder RotateTolerant(float tolerant) { _options.RotateTolerant = tolerant; return this; }

    public CaptchaBuilder ConcatTolerant(float tolerant) { _options.ConcatTolerant = tolerant; return this; }

    public CaptchaBuilder WordImageClickTolerant(float tolerant) { _options.WordImageClickTolerant = tolerant; return this; }

    public CaptchaBuilder Expire(CaptchaType captchaType, long expireMs)
    {
        _options.Expire[captchaType] = expireMs;
        return this;
    }

    public IImageCaptchaApplication Build()
    {
        var resourceStore = _resourceStore ?? new InMemoryResourceStore();
        if (resourceStore is not ICrudResourceStore crudStore)
        {
            throw new ArgumentException("Resource store must be of type ICrudResourceStore", nameof(resourceStore));
        }
        var resourceManager = new DefaultImageCaptchaResourceManager(crudStore);
        var cacheStore = _cacheStore ?? new MemoryCacheStore();
        var interceptor = _interceptor ?? EmptyCaptchaInterceptor.Instance;
        var imageTransform = _imageTransform ?? new Base64ImageTransform();
        var validator = _validator ?? new SimpleImageCaptchaValidator(_options);

        IImageCaptchaGenerator generator;
        if (_generator != null)
        {
            generator = _generator;
        }
        else
        {
            var multi = new MultiImageCaptchaGenerator();
            multi.SetImageResourceManager(resourceManager);
            multi.SetImageTransform(imageTransform);
            multi.SetInterceptor(interceptor);
            generator = multi;
        }

        generator.Init();

        // Add default templates
        if (_options.InitDefaultResource)
        {
            DefaultBuiltInResources.AddDefaultResources(resourceStore, _options.DefaultResourcePrefix);
        }

        // Add custom resources
        if (resourceStore is ICrudResourceStore store)
        {
            foreach (var (type, resource) in _resources)
                store.AddResource(type, resource);
            foreach (var (type, template) in _templates)
            {
                store.AddTemplate(type, template);
            }
        }

        // Wrap with cache if enabled
        if (_options.LocalCacheEnabled)
        {
            var cacheGen = new CacheImageCaptchaGenerator(generator, _options.LocalCacheSize,
                _options.LocalCacheWaitTime, _options.LocalCachePeriod, _options.LocalCacheExpireTime);
            if (_options.LocalCacheIgnoredCacheFields != null)
                cacheGen.IgnoredCacheFields = _options.LocalCacheIgnoredCacheFields;
            generator = cacheGen;
            generator.Init();
        }

        return new DefaultImageCaptchaApplication(generator, validator, cacheStore, _options, interceptor);
    }
}

public static class DefaultBuiltInResources
{
    public static void AddDefaultResources(IResourceStore store, string prefix)
    {
        if (store is not ICrudResourceStore crudStore) return;

        // 创建临时的资源管理器来使用 ResourceScanner
        var resourceManager = new DefaultImageCaptchaResourceManager(crudStore);
        var scanner = new ResourceScanner(resourceManager);

        // 扫描当前程序集（Tianai.Captcha.Core）中的资源
        var assembly = typeof(DefaultBuiltInResources).Assembly;
        scanner.ScanAssembly(assembly, prefix);
    }
}
