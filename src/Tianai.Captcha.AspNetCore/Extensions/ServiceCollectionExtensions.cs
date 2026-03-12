using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Tianai.Captcha.AspNetCore.Cache;
using Tianai.Captcha.AspNetCore.Configuration;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Cache.Impl;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;
using Tianai.Captcha.Core.Generator.Impl;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Pregeneration;
using Tianai.Captcha.Core.Pregeneration.Impl;
using Tianai.Captcha.Core.Resource;
using Tianai.Captcha.Core.Resource.Impl;
using Tianai.Captcha.Core.Validator;
using Tianai.Captcha.Core.Validator.Impl;

namespace Tianai.Captcha.AspNetCore.Extensions;

public interface ITianaiCaptchaBuilder
{
    IServiceCollection Services { get; }
}

internal class TianaiCaptchaBuilder : ITianaiCaptchaBuilder
{
    public IServiceCollection Services { get; }
    public TianaiCaptchaBuilder(IServiceCollection services) { Services = services; }
}

public static class ServiceCollectionExtensions
{
    public static ITianaiCaptchaBuilder AddTianaiCaptcha(this IServiceCollection services, Action<TianaiCaptchaOptions>? configure = null)
    {
        // Configure options
        if (configure != null)
            services.Configure(configure);
        // 不要使用空的配置回调，否则会覆盖之前添加的配置
        // else
        //     services.Configure<TianaiCaptchaOptions>(_ => { });

        // Resource layer
        services.TryAddSingleton<IResourceStore, InMemoryResourceStore>();
        services.TryAddSingleton<IImageCaptchaResourceManager>(sp =>
        {
            var store = sp.GetRequiredService<IResourceStore>();
            if (store is not ICrudResourceStore crudStore)
            {
                throw new ArgumentException("Resource store must be of type ICrudResourceStore");
            }
            return new DefaultImageCaptchaResourceManager(crudStore);
        });

        // Image transform
        services.TryAddSingleton<IImageTransform, Base64ImageTransform>();

        // Interceptor
        services.TryAddSingleton<ICaptchaInterceptor, EmptyCaptchaInterceptor>();

        // Validator
        services.TryAddSingleton<IImageCaptchaValidator, SimpleImageCaptchaValidator>();

        // Generator
        services.TryAddSingleton<IImageCaptchaGenerator>(sp =>
        {
            var resourceManager = sp.GetRequiredService<IImageCaptchaResourceManager>();
            var imageTransform = sp.GetRequiredService<IImageTransform>();
            var interceptor = sp.GetRequiredService<ICaptchaInterceptor>();
            var logger = sp.GetService<ILogger<MultiImageCaptchaGenerator>>();

            var multi = new MultiImageCaptchaGenerator(logger);
            multi.SetImageResourceManager(resourceManager);
            multi.SetImageTransform(imageTransform);
            multi.SetInterceptor(interceptor);

            // Register any additional providers from DI
            var providers = sp.GetServices<IImageCaptchaGeneratorProvider>();
            foreach (var provider in providers)
                multi.AddImageCaptchaGeneratorProvider(provider);

            multi.Init();
            return multi;
        });

        // Cache store - default to memory, can be overridden
        services.TryAddSingleton<ICacheStore>(sp =>
        {
            // Try to use IDistributedCache if available (and not the default MemoryDistributedCache)
            var distributedCache = sp.GetService<IDistributedCache>();
            if (distributedCache != null)
                return new DistributedCacheStore(distributedCache);
            return new MemoryCacheStore();
        });

        // 注册预生成池相关服务
        services.TryAddSingleton<ICaptchaPregenerationPool>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TianaiCaptchaOptions>>().Value;
            if (!options.PregenerationPoolEnabled)
                return null;
            return new MemoryCaptchaPregenerationPool(
                options.PregenerationPoolMaxCapacity,
                options.PregenerationPoolMinThreshold);
        });

        // 注册预生成服务作为后台服务
        services.AddHostedService<CaptchaPregenerationService>();



        // Application - 使用 AddSingleton 而不是 TryAddSingleton，确保在所有配置完成后再注册
        services.AddSingleton<IImageCaptchaApplication>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TianaiCaptchaOptions>>().Value;
            var generator = sp.GetRequiredService<IImageCaptchaGenerator>();
            var validator = sp.GetRequiredService<IImageCaptchaValidator>();
            var cacheStore = sp.GetRequiredService<ICacheStore>();
            var interceptor = sp.GetRequiredService<ICaptchaInterceptor>();
            var resourceManager = sp.GetRequiredService<IImageCaptchaResourceManager>();
            var pregenerationPool = options.PregenerationPoolEnabled ? sp.GetRequiredService<ICaptchaPregenerationPool>() : null;
            var logger = sp.GetService<ILogger<DefaultImageCaptchaApplication>>();

            // Initialize default resources
            if (options.InitDefaultResource)
            {
                DefaultBuiltInResources.AddDefaultResources(resourceManager.GetResourceStore(), options.DefaultResourcePrefix);
            }

            // 从服务容器中获取所有注册的 Assembly 实例
            var assemblies = sp.GetServices<Assembly>().ToList();
            Debug.WriteLine($"从服务容器中获取到 {assemblies.Count} 个程序集");
            foreach (var assem in assemblies)
            {
                Debug.WriteLine($"  - {assem.FullName}");
            }

            // Register custom assemblies with EmbeddedResourceProvider
            var embeddedProvider = resourceManager.ListResourceProviders()
                .OfType<EmbeddedResourceProvider>().FirstOrDefault();
            if (embeddedProvider != null)
            {
                // 注册通过 AddSingleton<Assembly> 方法添加的程序集
                if (assemblies.Count > 0)
                {
                    Debug.WriteLine($"注册了 {assemblies.Count} 个程序集（从服务容器）");
                    foreach (var assembly in assemblies)
                    {
                        Debug.WriteLine($"注册程序集: {assembly.FullName}");
                        embeddedProvider.RegisterAssembly(assembly);
                    }
                }
            }



            // Add custom resources and templates
            var store = resourceManager.GetResourceStore();
            if (store is ICrudResourceStore crudStore)
            {
                foreach (var (type, resource) in options.CustomBackgroundResources)
                {
                    crudStore.AddResource(type, resource);
                }
                foreach (var (type, template) in options.CustomTemplateResources)
                {
                    crudStore.AddTemplate(type, template);
                }
                foreach (var (type, resource) in options.CustomFontResources)
                {
                    crudStore.AddFontResource(type, resource);
                }
            }

            // Wrap with cache if enabled
            var finalGenerator = generator;
            if (options.LocalCacheEnabled)
            {
                var cacheGen = new CacheImageCaptchaGenerator(generator, options.LocalCacheSize,
                    options.LocalCacheWaitTime, options.LocalCachePeriod, options.LocalCacheExpireTime);
                if (options.LocalCacheIgnoredCacheFields != null)
                    cacheGen.IgnoredCacheFields = options.LocalCacheIgnoredCacheFields;
                finalGenerator = cacheGen;
                finalGenerator.Init();
            }

            return new DefaultImageCaptchaApplication(finalGenerator, validator, cacheStore, options, interceptor, pregenerationPool, logger);
        });

        return new TianaiCaptchaBuilder(services);
    }

    public static ITianaiCaptchaBuilder AddDistributedCache(this ITianaiCaptchaBuilder builder)
    {
        builder.Services.RemoveAll<ICacheStore>();
        builder.Services.AddSingleton<ICacheStore>(sp =>
        {
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new DistributedCacheStore(cache);
        });
        return builder;
    }

    // 用于跟踪已注册的程序集，防止重复注册
    private static readonly HashSet<Assembly> _registeredAssemblies = new HashSet<Assembly>();
    
    // 用于存储需要扫描的目录路径，确保只创建一个 DirectoryScannerService
    private static readonly List<string> _directoriesToScan = new List<string>();

    /// <summary>
    /// 注册额外程序集用于嵌入资源查找（对应 Java classpath 资源搜索）
    /// </summary>
    public static ITianaiCaptchaBuilder ScanAssembly(this ITianaiCaptchaBuilder builder, Assembly assembly)
    {
        // 检查程序集是否已经注册过，防止重复注册
        if (!_registeredAssemblies.Add(assembly))
        {
            Debug.WriteLine($"程序集已注册，跳过: {assembly.FullName}");
            return builder;
        }

        // 直接向服务容器中添加一个单例来存储程序集
        builder.Services.AddSingleton(assembly);
        
        // 尝试立即注册程序集到 EmbeddedResourceProvider
        // 注意：这只有在 EmbeddedResourceProvider 已经被注册的情况下才会生效
        // 否则会在 IImageCaptchaApplication 初始化时再次注册
        try
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var resourceManager = serviceProvider.GetService<IImageCaptchaResourceManager>();
            
            Debug.WriteLine($"ScanAssembly 被调用，程序集：{assembly.FullName}");
            
            if (resourceManager != null)
            {
                resourceManager.RegisterAssembly(assembly);
            }
        }
        catch (Exception ex)
        {
            // 忽略异常，因为服务可能还没有完全注册
            Debug.WriteLine($"立即注册程序集失败，将在应用初始化时注册: {ex.Message}");
        }
        
        return builder;
    }

    /// <summary>
    /// 添加背景图资源
    /// </summary>
    public static ITianaiCaptchaBuilder AddBackgroundResource(this ITianaiCaptchaBuilder builder, CaptchaType type, CaptchaResource resource)
    {
        builder.Services.PostConfigure<TianaiCaptchaOptions>(options =>
        {
            options.CustomBackgroundResources.Add((type, resource));
        });
        return builder;
    }

    /// <summary>
    /// 添加字体资源
    /// </summary>
    public static ITianaiCaptchaBuilder AddFontResource(this ITianaiCaptchaBuilder builder, string type, CaptchaResource resource)
    {
        builder.Services.PostConfigure<TianaiCaptchaOptions>(options =>
        {
            options.CustomFontResources.Add((type, resource));
        });
        return builder;
    }

    /// <summary>
    /// 添加模板资源
    /// </summary>
    public static ITianaiCaptchaBuilder AddTemplateResource(this ITianaiCaptchaBuilder builder, CaptchaType type, ResourceMap template)
    {
        builder.Services.PostConfigure<TianaiCaptchaOptions>(options =>
        {
            options.CustomTemplateResources.Add((type, template));
        });
        return builder;
    }



    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    public static ITianaiCaptchaBuilder ScanDirectory(this ITianaiCaptchaBuilder builder, string directoryPath)
    {
        // 添加目录路径到列表
        _directoriesToScan.Add(directoryPath);
        
        // 检查是否已经添加了 DirectoryScannerService
        var serviceDescriptor = builder.Services.FirstOrDefault(descriptor => 
            descriptor.ServiceType == typeof(IHostedService) && 
            descriptor.ImplementationType == typeof(DirectoryScannerService));
        
        // 如果还没有添加服务，则添加一次
        if (serviceDescriptor == null)
        {
            builder.Services.AddSingleton<IHostedService, DirectoryScannerService>(sp =>
            {
                var resourceManager = sp.GetRequiredService<IImageCaptchaResourceManager>();
                return new DirectoryScannerService(resourceManager, _directoriesToScan);
            });
        }
        
        return builder;
    }

    /// <summary>
    /// 目录扫描服务
    /// </summary>
    private class DirectoryScannerService : IHostedService
    {
        private readonly IImageCaptchaResourceManager _resourceManager;
        private readonly List<string> _directoryPaths;

        public DirectoryScannerService(IImageCaptchaResourceManager resourceManager, List<string> directoryPaths)
        {
            _resourceManager = resourceManager;
            _directoryPaths = directoryPaths;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var fileProvider = _resourceManager.ListResourceProviders()
                .OfType<FileResourceProvider>().FirstOrDefault();
            if (fileProvider != null)
            {
                foreach (var directoryPath in _directoryPaths)
                {
                    fileProvider.ScanDirectory(directoryPath);
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }


}
