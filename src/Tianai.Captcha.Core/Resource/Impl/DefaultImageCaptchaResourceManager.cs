using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

public class DefaultImageCaptchaResourceManager : IImageCaptchaResourceManager
{
    private ICrudResourceStore _resourceStore;
    private readonly List<IResourceProvider> _providers = new();

    public DefaultImageCaptchaResourceManager(ICrudResourceStore? resourceStore = null)
    {
        _resourceStore = resourceStore ?? new InMemoryResourceStore();
        _resourceStore.Init(this);

        // Register default providers
        var embeddedProvider = new EmbeddedResourceProvider();
        embeddedProvider.SetResourceManager(this);
        RegisterResourceProvider(embeddedProvider);
        var fileProvider = new FileResourceProvider();
        fileProvider.SetResourceManager(this);
        RegisterResourceProvider(fileProvider);
        var uriProvider = new UriResourceProvider();
        uriProvider.SetResourceManager(this);
        RegisterResourceProvider(uriProvider);
    }

    public ResourceMap RandomGetTemplate(CaptchaType type, string? tag)
    {
        var list = RandomGetTemplate(type, tag, 1);
        return list.Count > 0 ? list[0] : throw new ImageCaptchaException($"No template found for type={type}, tag={tag}");
    }

    public CaptchaResource RandomGetResource(CaptchaType type, string? tag)
    {
        var list = RandomGetResource(type, tag, 1);
        return list.Count > 0 ? list[0] : throw new ImageCaptchaException($"No resource found for type={type}, tag={tag}");
    }

    public List<ResourceMap> RandomGetTemplate(CaptchaType type, string? tag, int quantity)
        => _resourceStore.RandomGetTemplateResourceByTypeAndTag(type, tag, quantity);

    public List<CaptchaResource> RandomGetResource(CaptchaType type, string? tag, int quantity)
        => _resourceStore.RandomGetBackgroundResourceByTypeAndTag(type, tag, quantity);



    public Stream GetResourceStream(CaptchaResource resource)
    {
        foreach (var provider in _providers)
        {
            if (provider.Supported(resource))
                return provider.GetResourceStream(resource);
        }
        throw new ImageCaptchaException($"No provider found for resource type={resource.Type}, data={resource.Data}");
    }

    public IReadOnlyList<IResourceProvider> ListResourceProviders() => _providers;

    public void RegisterResourceProvider(IResourceProvider provider)
    {
        _providers.Add(provider);
        // 如果是 EmbeddedResourceProvider、FileResourceProvider 或 UriResourceProvider，设置资源管理器引用
        if (provider is EmbeddedResourceProvider embeddedProvider)
        {
            embeddedProvider.SetResourceManager(this);
        }
        else if (provider is FileResourceProvider fileProvider)
        {
            fileProvider.SetResourceManager(this);
        }
        else if (provider is UriResourceProvider uriProvider)
        {
            uriProvider.SetResourceManager(this);
        }
    }

    public bool DeleteResourceProviderByName(string name)
    {
        var provider = _providers.FirstOrDefault(p => p.Name == name);
        if (provider != null) { _providers.Remove(provider); return true; }
        return false;
    }

    public void SetResourceStore(IResourceStore store)
    {
        if (store is not ICrudResourceStore crudStore)
        {
            throw new ArgumentException("Resource store must be of type ICrudResourceStore", nameof(store));
        }
        _resourceStore = crudStore;
        _resourceStore.Init(this);
    }

    public IResourceStore GetResourceStore() => _resourceStore;

    /// <summary>
    /// 注册程序集并自动扫描资源
    /// </summary>
    public void RegisterAssembly(System.Reflection.Assembly assembly)
    {
        var embeddedProvider = _providers.OfType<EmbeddedResourceProvider>().FirstOrDefault();
        if (embeddedProvider != null)
        {
            embeddedProvider.RegisterAssembly(assembly);
        }
    }

    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    public void ScanDirectory(string directoryPath)
    {
        var fileProvider = _providers.OfType<FileResourceProvider>().FirstOrDefault();
        if (fileProvider != null)
        {
            fileProvider.ScanDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 添加背景图资源
    /// </summary>
    public void AddBackgroundResource(CaptchaType type, CaptchaResource resource)
    {
        _resourceStore.AddBackgroundResource(type, resource);
    }

    /// <summary>
    /// 添加模板资源
    /// </summary>
    public void AddTemplateResource(CaptchaType type, ResourceMap template)
    {
        _resourceStore.AddTemplateResource(type, template);
    }

    /// <summary>
    /// 添加字体资源
    /// </summary>
    public void AddFontResource(CaptchaType type, CaptchaResource resource)
    {
        _resourceStore.AddFontResource("Fonts", resource);
    }

    /// <summary>
    /// 删除背景图资源
    /// </summary>
    public CaptchaResource? DeleteBackgroundResource(CaptchaType type, string id)
    {
        return _resourceStore.DeleteBackgroundResource(type, id);
    }

    /// <summary>
    /// 列出背景图资源
    /// </summary>
    public List<CaptchaResource> ListBackgroundResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        return _resourceStore.ListBackgroundResourcesByTypeAndTag(type, tag);
    }

    /// <summary>
    /// 删除模板资源
    /// </summary>
    public ResourceMap? DeleteTemplateResource(CaptchaType type, string id)
    {
        return _resourceStore.DeleteTemplateResource(type, id);
    }

    /// <summary>
    /// 列出模板资源
    /// </summary>
    public List<ResourceMap> ListTemplateResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        return _resourceStore.ListTemplateResourcesByTypeAndTag(type, tag);
    }

    /// <summary>
    /// 删除字体资源
    /// </summary>
    public CaptchaResource? DeleteFontResource(string type, string id)
    {
        return _resourceStore.DeleteFontResource(type, id);
    }

    /// <summary>
    /// 列出字体资源
    /// </summary>
    public List<CaptchaResource> ListFontResourcesByTypeAndTag(string type, string? tag)
    {
        return _resourceStore.ListFontResourcesByTypeAndTag(type, tag);
    }
}
