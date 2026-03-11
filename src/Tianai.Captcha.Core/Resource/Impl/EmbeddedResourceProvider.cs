using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

public class EmbeddedResourceProvider : IResourceProvider
{
    private readonly List<Assembly> _assemblies = new() { typeof(EmbeddedResourceProvider).Assembly };
    private IImageCaptchaResourceManager? _resourceManager;
    private readonly ILogger<EmbeddedResourceProvider> _logger;

    public string Name => "embedded";

    public EmbeddedResourceProvider(ILogger<EmbeddedResourceProvider>? logger = null)
    {
        _logger = logger ?? NullLogger<EmbeddedResourceProvider>.Instance;
    }

    public bool Supported(CaptchaResource resource)
        => "embedded".Equals(resource.Type, StringComparison.OrdinalIgnoreCase)
        || "classpath".Equals(resource.Type, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 设置资源管理器
    /// </summary>
    public void SetResourceManager(IImageCaptchaResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    /// <summary>
    /// 注册额外的程序集用于嵌入资源查找（对应 Java 的 classpath 资源搜索）
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
            // 自动扫描并添加资源
            if (_resourceManager != null)
            {
                _logger.LogInformation("开始扫描程序集: {AssemblyFullName}", assembly.FullName);
                var resourceNames = assembly.GetManifestResourceNames();
                _logger.LogInformation("程序集包含 {ResourceCount} 个资源", resourceNames.Length);
                foreach (var name in resourceNames)
                {
                    if (name.Contains("BgImages") || name.Contains("Templates") || name.Contains("Fonts"))
                    {
                        _logger.LogDebug("找到资源: {ResourceName}", name);
                    }
                }
                ScanAndAddResources(assembly);
            }
        }
    }

    public Stream GetResourceStream(CaptchaResource resource)
    {
        var data = resource.Data ?? throw new ImageCaptchaException("Resource data is null");
        var resourceName = data.Replace('/', '.').Replace('\\', '.');
        if (resourceName.StartsWith('.')) resourceName = resourceName[1..];

        // If Extra carries a specific Assembly, search it first
        if (resource.Extra is Assembly specificAssembly)
        {
            var stream = TryGetStream(specificAssembly, resourceName);
            if (stream != null) return stream;
        }

        // Search all registered assemblies
        foreach (var assembly in _assemblies)
        {
            var stream = TryGetStream(assembly, resourceName);
            if (stream != null) return stream;
        }

        throw new ImageCaptchaException($"Embedded resource not found: {data}");
    }

    private static Stream? TryGetStream(Assembly assembly, string resourceName)
    {
        // Try exact match
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null) return stream;

        // Try with assembly default namespace prefix
        var prefix = assembly.GetName().Name ?? "";
        var fullName = $"{prefix}.{resourceName}";
        stream = assembly.GetManifestResourceStream(fullName);
        if (stream != null) return stream;

        // Try suffix match
        var names = assembly.GetManifestResourceNames();
        var match = names.FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            stream = assembly.GetManifestResourceStream(match);
            if (stream != null) return stream;
        }

        return null;
    }

    /// <summary>
    /// 扫描并添加程序集中的资源
    /// </summary>
    private void ScanAndAddResources(Assembly assembly)
    {
        _logger.LogInformation("开始扫描程序集中的资源...");
        var scanner = new ResourceScanner(_resourceManager);
        scanner.ScanAssembly(assembly);
        _logger.LogInformation("程序集资源扫描完成");
    }
}

public class FileResourceProvider : IResourceProvider
{
    private readonly List<string> _directories = new();
    private IImageCaptchaResourceManager? _resourceManager;
    private readonly ILogger<FileResourceProvider> _logger;

    public string Name => "file";

    public FileResourceProvider(ILogger<FileResourceProvider>? logger = null)
    {
        _logger = logger ?? NullLogger<FileResourceProvider>.Instance;
    }

    public bool Supported(CaptchaResource resource)
        => "file".Equals(resource.Type, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 设置资源管理器
    /// </summary>
    public void SetResourceManager(IImageCaptchaResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    public Stream GetResourceStream(CaptchaResource resource)
    {
        var path = resource.Data ?? throw new ImageCaptchaException("Resource data is null");
        if (!File.Exists(path))
            throw new ImageCaptchaException($"File not found: {path}");
        return File.OpenRead(path);
    }

    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    public void ScanDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new ImageCaptchaException($"Directory not found: {directoryPath}");

        if (_resourceManager == null)
            throw new ImageCaptchaException("Resource manager not set");

        // 避免重复扫描
        if (!_directories.Contains(directoryPath))
        {
            _directories.Add(directoryPath);
            var scanner = new ResourceScanner(_resourceManager);
            scanner.ScanDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 获取所有已扫描的目录
    /// </summary>
    public IReadOnlyList<string> GetScannedDirectories()
    {
        return _directories.AsReadOnly();
    }

    /// <summary>
    /// 重新扫描所有已注册的目录
    /// </summary>
    public void RescanDirectories()
    {
        if (_resourceManager == null)
            throw new ImageCaptchaException("Resource manager not set");

        var scanner = new ResourceScanner(_resourceManager);
        foreach (var directory in _directories)
        {
            if (Directory.Exists(directory))
            {
                scanner.ScanDirectory(directory);
            }
        }
    }
}

public class UriResourceProvider : IResourceProvider
{
    private static readonly HttpClient HttpClient = new();
    private readonly Dictionary<string, byte[]> _cache = new();
    private readonly object _cacheLock = new();
    private IImageCaptchaResourceManager? _resourceManager;
    private readonly ILogger<UriResourceProvider> _logger;

    public string Name => "url";

    public UriResourceProvider(ILogger<UriResourceProvider>? logger = null)
    {
        _logger = logger ?? NullLogger<UriResourceProvider>.Instance;
    }

    public bool Supported(CaptchaResource resource)
        => "url".Equals(resource.Type, StringComparison.OrdinalIgnoreCase)
        || "http".Equals(resource.Type, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 设置资源管理器
    /// </summary>
    public void SetResourceManager(IImageCaptchaResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    public Stream GetResourceStream(CaptchaResource resource)
    {
        var url = resource.Data ?? throw new ImageCaptchaException("Resource data is null");
        
        // 尝试从缓存获取
        if (TryGetFromCache(url, out var cachedData))
        {
            return new MemoryStream(cachedData);
        }

        try
        {
            // 异步加载资源
            var data = HttpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
            
            // 存入缓存
            AddToCache(url, data);
            
            return new MemoryStream(data);
        }
        catch (Exception ex)
        {
            throw new ImageCaptchaException($"Failed to load URL resource: {url}", ex);
        }
    }

    /// <summary>
    /// 批量加载 URL 资源
    /// </summary>
    public void LoadBatchResources(IEnumerable<string> urls, string resourceType = "Fonts")
    {
        if (_resourceManager == null)
            throw new ImageCaptchaException("Resource manager not set");

        var store = _resourceManager.GetResourceStore() as ICrudResourceStore;
        if (store == null)
            throw new ImageCaptchaException("Resource store is not a ICrudResourceStore");

        foreach (var url in urls)
        {
            try
            {
                var captchaResource = new CaptchaResource
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "url",
                    Data = url,
                    Tag = CommonConstant.DefaultTag
                };
                store.AddFontResource(resourceType, captchaResource);
            }
            catch (Exception ex)
            {
                // 记录错误但继续处理其他资源
                _logger.LogError(ex, "Failed to load resource from {Url}", url);
            }
        }
    }

    /// <summary>
    /// 从缓存获取资源
    /// </summary>
    private bool TryGetFromCache(string url, out byte[] data)
    {
        lock (_cacheLock)
        {
            return _cache.TryGetValue(url, out data);
        }
    }

    /// <summary>
    /// 添加资源到缓存
    /// </summary>
    private void AddToCache(string url, byte[] data)
    {
        lock (_cacheLock)
        {
            _cache[url] = data;
        }
    }

    /// <summary>
    /// 清理缓存
    /// </summary>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
        }
    }
}
