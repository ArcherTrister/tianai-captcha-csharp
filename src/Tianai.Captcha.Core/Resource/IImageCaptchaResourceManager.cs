using System.Reflection;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource;

public interface IImageCaptchaResourceManager
{
    ResourceMap RandomGetTemplate(CaptchaType type, string? tag);
    CaptchaResource RandomGetResource(CaptchaType type, string? tag);
    List<ResourceMap> RandomGetTemplate(CaptchaType type, string? tag, int quantity);
    List<CaptchaResource> RandomGetResource(CaptchaType type, string? tag, int quantity);
    Stream GetResourceStream(CaptchaResource resource);
    IReadOnlyList<IResourceProvider> ListResourceProviders();
    void RegisterResourceProvider(IResourceProvider provider);
    bool DeleteResourceProviderByName(string name);
    void SetResourceStore(IResourceStore store);
    IResourceStore GetResourceStore();

    /// <summary>
    /// 注册程序集并自动扫描资源
    /// </summary>
    void RegisterAssembly(Assembly assembly);

    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    void ScanDirectory(string directoryPath);

    /// <summary>
    /// 添加背景图资源
    /// </summary>
    void AddBackgroundResource(CaptchaType type, CaptchaResource resource);

    /// <summary>
    /// 删除背景图资源
    /// </summary>
    CaptchaResource? DeleteBackgroundResource(CaptchaType type, string id);

    /// <summary>
    /// 列出背景图资源
    /// </summary>
    List<CaptchaResource> ListBackgroundResourcesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 添加模板资源
    /// </summary>
    void AddTemplateResource(CaptchaType type, ResourceMap template);

    /// <summary>
    /// 删除模板资源
    /// </summary>
    ResourceMap? DeleteTemplateResource(CaptchaType type, string id);

    /// <summary>
    /// 列出模板资源
    /// </summary>
    List<ResourceMap> ListTemplateResourcesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 添加字体资源
    /// </summary>
    void AddFontResource(CaptchaType type, CaptchaResource resource);

    /// <summary>
    /// 删除字体资源
    /// </summary>
    CaptchaResource? DeleteFontResource(string type, string id);

    /// <summary>
    /// 列出字体资源
    /// </summary>
    List<CaptchaResource> ListFontResourcesByTypeAndTag(string type, string? tag);


}
