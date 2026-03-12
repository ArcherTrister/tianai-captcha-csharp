using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

public class DefaultImageCaptchaResourceManager : IImageCaptchaResourceManager
{
    private ICrudResourceStore _resourceStore;
    private readonly List<IResourceProvider> _providers = new();
    private readonly ILogger<DefaultImageCaptchaResourceManager> _logger;

    public DefaultImageCaptchaResourceManager(ICrudResourceStore? resourceStore = null, ILogger<DefaultImageCaptchaResourceManager>? logger = null)
    {
        _resourceStore = resourceStore ?? new InMemoryResourceStore();
        _resourceStore.Init(this);
        _logger = logger ?? NullLogger<DefaultImageCaptchaResourceManager>.Instance;

        // Register default providers
        _logger.LogInformation("开始注册默认资源提供者...");
        var embeddedProvider = new EmbeddedResourceProvider();
        embeddedProvider.SetResourceManager(this);
        RegisterResourceProvider(embeddedProvider);
        var fileProvider = new FileResourceProvider();
        fileProvider.SetResourceManager(this);
        RegisterResourceProvider(fileProvider);
        var uriProvider = new UriResourceProvider();
        uriProvider.SetResourceManager(this);
        RegisterResourceProvider(uriProvider);
        _logger.LogInformation("默认资源提供者注册完成");
    }

    public ResourceMap RandomGetTemplate(CaptchaType type, string? tag)
    {
        _logger.LogDebug("随机获取模板资源: type={Type}, tag={Tag}", type, tag);
        var list = RandomGetTemplate(type, tag, 1);
        if (list.Count > 0)
        {
            _logger.LogDebug("成功获取模板资源: id={Id}", list[0].Id);
            return list[0];
        }
        _logger.LogError("未找到模板资源: type={Type}, tag={Tag}", type, tag);
        throw new ImageCaptchaException($"No template found for type={type}, tag={tag}");
    }

    public CaptchaResource RandomGetResource(CaptchaType type, string? tag)
    {
        _logger.LogDebug("随机获取背景资源: type={Type}, tag={Tag}", type, tag);
        var list = RandomGetResource(type, tag, 1);
        if (list.Count > 0)
        {
            _logger.LogDebug("成功获取背景资源: id={Id}, data={Data}", list[0].Id, list[0].Data);
            return list[0];
        }
        _logger.LogError("未找到背景资源: type={Type}, tag={Tag}", type, tag);
        throw new ImageCaptchaException($"No resource found for type={type}, tag={tag}");
    }

    public List<ResourceMap> RandomGetTemplate(CaptchaType type, string? tag, int quantity)
    {
        _logger.LogDebug("批量随机获取模板资源: type={Type}, tag={Tag}, quantity={Quantity}", type, tag, quantity);
        var result = _resourceStore.RandomGetTemplateResourceByTypeAndTag(type, tag, quantity);
        _logger.LogDebug("批量获取模板资源完成: 数量={Count}", result.Count);
        return result;
    }

    public List<CaptchaResource> RandomGetResource(CaptchaType type, string? tag, int quantity)
    {
        _logger.LogDebug("批量随机获取背景资源: type={Type}, tag={Tag}, quantity={Quantity}", type, tag, quantity);
        var result = _resourceStore.RandomGetBackgroundResourceByTypeAndTag(type, tag, quantity);
        _logger.LogDebug("批量获取背景资源完成: 数量={Count}", result.Count);
        return result;
    }

    public Stream GetResourceStream(CaptchaResource resource)
    {
        _logger.LogDebug("获取资源流: type={Type}, data={Data}", resource.Type, resource.Data);
        foreach (var provider in _providers)
        {
            if (provider.Supported(resource))
            {
                _logger.LogDebug("使用资源提供者: {ProviderName}", provider.Name);
                return provider.GetResourceStream(resource);
            }
        }
        _logger.LogError("未找到支持的资源提供者: type={Type}, data={Data}", resource.Type, resource.Data);
        throw new ImageCaptchaException($"No provider found for resource type={resource.Type}, data={resource.Data}");
    }

    public IReadOnlyList<IResourceProvider> ListResourceProviders() => _providers;

    public void RegisterResourceProvider(IResourceProvider provider)
    {
        _logger.LogInformation("注册资源提供者: {ProviderName}", provider.Name);
        _providers.Add(provider);
        // 如果是 EmbeddedResourceProvider、FileResourceProvider 或 UriResourceProvider，设置资源管理器引用
        if (provider is EmbeddedResourceProvider embeddedProvider)
        {
            embeddedProvider.SetResourceManager(this);
            _logger.LogDebug("设置嵌入式资源提供者的资源管理器引用");
        }
        else if (provider is FileResourceProvider fileProvider)
        {
            fileProvider.SetResourceManager(this);
            _logger.LogDebug("设置文件资源提供者的资源管理器引用");
        }
        else if (provider is UriResourceProvider uriProvider)
        {
            uriProvider.SetResourceManager(this);
            _logger.LogDebug("设置 URI 资源提供者的资源管理器引用");
        }
        _logger.LogInformation("资源提供者注册完成: {ProviderName}", provider.Name);
    }

    public bool DeleteResourceProviderByName(string name)
    {
        _logger.LogInformation("删除资源提供者: {ProviderName}", name);
        var provider = _providers.FirstOrDefault(p => p.Name == name);
        if (provider != null)
        {
            _providers.Remove(provider);
            _logger.LogInformation("资源提供者删除成功: {ProviderName}", name);
            return true;
        }
        _logger.LogWarning("资源提供者不存在: {ProviderName}", name);
        return false;
    }

    public void SetResourceStore(IResourceStore store)
    {
        _logger.LogInformation("设置资源存储");
        if (store is not ICrudResourceStore crudStore)
        {
            _logger.LogError("资源存储类型错误，必须是 ICrudResourceStore 类型");
            throw new ArgumentException("Resource store must be of type ICrudResourceStore", nameof(store));
        }
        _resourceStore = crudStore;
        _resourceStore.Init(this);
        _logger.LogInformation("资源存储设置完成");
    }

    public IResourceStore GetResourceStore() => _resourceStore;

    /// <summary>
    /// 注册程序集并自动扫描资源
    /// </summary>
    public void RegisterAssembly(System.Reflection.Assembly assembly)
    {
        _logger.LogInformation("注册程序集: {AssemblyName}", assembly.GetName().Name);
        var embeddedProvider = _providers.OfType<EmbeddedResourceProvider>().FirstOrDefault();
        if (embeddedProvider != null)
        {
            embeddedProvider.RegisterAssembly(assembly);
            _logger.LogInformation("程序集注册完成: {AssemblyName}", assembly.GetName().Name);
        }
        else
        {
            _logger.LogWarning("未找到嵌入式资源提供者");
        }
    }

    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    public void ScanDirectory(string directoryPath)
    {
        _logger.LogInformation("扫描目录: {DirectoryPath}", directoryPath);
        var fileProvider = _providers.OfType<FileResourceProvider>().FirstOrDefault();
        if (fileProvider != null)
        {
            fileProvider.ScanDirectory(directoryPath);
            _logger.LogInformation("目录扫描完成: {DirectoryPath}", directoryPath);
        }
        else
        {
            _logger.LogWarning("未找到文件资源提供者");
        }
    }

    /// <summary>
    /// 添加背景图资源
    /// </summary>
    public void AddBackgroundResource(CaptchaType type, CaptchaResource resource)
    {
        _logger.LogDebug("添加背景图资源: type={Type}, id={Id}, data={Data}", type, resource.Id, resource.Data);
        _resourceStore.AddBackgroundResource(type, resource);
        _logger.LogDebug("背景图资源添加完成: type={Type}, id={Id}", type, resource.Id);
    }

    /// <summary>
    /// 添加模板资源
    /// </summary>
    public void AddTemplateResource(CaptchaType type, ResourceMap template)
    {
        _logger.LogDebug("添加模板资源: type={Type}, id={Id}", type, template.Id);
        _resourceStore.AddTemplateResource(type, template);
        _logger.LogDebug("模板资源添加完成: type={Type}, id={Id}", type, template.Id);
    }

    /// <summary>
    /// 添加字体资源
    /// </summary>
    public void AddFontResource(CaptchaType type, CaptchaResource resource)
    {
        _logger.LogDebug("添加字体资源: id={Id}, data={Data}", resource.Id, resource.Data);
        _resourceStore.AddFontResource("Fonts", resource);
        _logger.LogDebug("字体资源添加完成: id={Id}", resource.Id);
    }

    /// <summary>
    /// 删除背景图资源
    /// </summary>
    public CaptchaResource? DeleteBackgroundResource(CaptchaType type, string id)
    {
        _logger.LogDebug("删除背景图资源: type={Type}, id={Id}", type, id);
        var result = _resourceStore.DeleteBackgroundResource(type, id);
        if (result != null)
        {
            _logger.LogDebug("背景图资源删除成功: type={Type}, id={Id}", type, id);
        }
        else
        {
            _logger.LogDebug("背景图资源不存在: type={Type}, id={Id}", type, id);
        }
        return result;
    }

    /// <summary>
    /// 列出背景图资源
    /// </summary>
    public List<CaptchaResource> ListBackgroundResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        _logger.LogDebug("列出背景图资源: type={Type}, tag={Tag}", type, tag);
        var result = _resourceStore.ListBackgroundResourcesByTypeAndTag(type, tag);
        _logger.LogDebug("背景图资源列出完成: 数量={Count}", result.Count);
        return result;
    }

    /// <summary>
    /// 删除模板资源
    /// </summary>
    public ResourceMap? DeleteTemplateResource(CaptchaType type, string id)
    {
        _logger.LogDebug("删除模板资源: type={Type}, id={Id}", type, id);
        var result = _resourceStore.DeleteTemplateResource(type, id);
        if (result != null)
        {
            _logger.LogDebug("模板资源删除成功: type={Type}, id={Id}", type, id);
        }
        else
        {
            _logger.LogDebug("模板资源不存在: type={Type}, id={Id}", type, id);
        }
        return result;
    }

    /// <summary>
    /// 列出模板资源
    /// </summary>
    public List<ResourceMap> ListTemplateResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        _logger.LogDebug("列出模板资源: type={Type}, tag={Tag}", type, tag);
        var result = _resourceStore.ListTemplateResourcesByTypeAndTag(type, tag);
        _logger.LogDebug("模板资源列出完成: 数量={Count}", result.Count);
        return result;
    }

    /// <summary>
    /// 删除字体资源
    /// </summary>
    public CaptchaResource? DeleteFontResource(string type, string id)
    {
        _logger.LogDebug("删除字体资源: type={Type}, id={Id}", type, id);
        var result = _resourceStore.DeleteFontResource(type, id);
        if (result != null)
        {
            _logger.LogDebug("字体资源删除成功: type={Type}, id={Id}", type, id);
        }
        else
        {
            _logger.LogDebug("字体资源不存在: type={Type}, id={Id}", type, id);
        }
        return result;
    }

    /// <summary>
    /// 列出字体资源
    /// </summary>
    public List<CaptchaResource> ListFontResourcesByTypeAndTag(string type, string? tag)
    {
        _logger.LogDebug("列出字体资源: type={Type}, tag={Tag}", type, tag);
        var result = _resourceStore.ListFontResourcesByTypeAndTag(type, tag);
        _logger.LogDebug("字体资源列出完成: 数量={Count}", result.Count);
        return result;
    }
}
