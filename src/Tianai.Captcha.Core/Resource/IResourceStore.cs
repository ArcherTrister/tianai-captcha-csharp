using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource;

public interface IResourceStore
{
    void Init(IImageCaptchaResourceManager resourceManager);
    List<CaptchaResource> RandomGetResourceByTypeAndTag(CaptchaType type, string? tag, int quantity);
    List<ResourceMap> RandomGetTemplateByTypeAndTag(CaptchaType type, string? tag, int quantity);
}

public interface ICrudResourceStore : IResourceStore
{
    /// <summary>
    /// 添加背景资源
    /// </summary>
    void AddBackgroundResource(CaptchaType type, CaptchaResource resource);

    /// <summary>
    /// 添加字体资源
    /// </summary>
    void AddFontResource(string type, CaptchaResource resource);

    /// <summary>
    /// 添加模板资源
    /// </summary>
    void AddTemplateResource(CaptchaType type, ResourceMap template);

    /// <summary>
    /// 删除背景资源
    /// </summary>
    CaptchaResource? DeleteBackgroundResource(CaptchaType type, string id);

    /// <summary>
    /// 删除字体资源
    /// </summary>
    CaptchaResource? DeleteFontResource(string type, string id);

    /// <summary>
    /// 删除模板资源
    /// </summary>
    ResourceMap? DeleteTemplateResource(CaptchaType type, string id);

    /// <summary>
    /// 列出背景资源
    /// </summary>
    List<CaptchaResource> ListBackgroundResourcesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 列出字体资源
    /// </summary>
    List<CaptchaResource> ListFontResourcesByTypeAndTag(string type, string? tag);

    /// <summary>
    /// 列出模板资源
    /// </summary>
    List<ResourceMap> ListTemplateResourcesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 随机获取背景资源
    /// </summary>
    List<CaptchaResource> RandomGetBackgroundResourceByTypeAndTag(CaptchaType type, string? tag, int quantity);

    /// <summary>
    /// 随机获取字体资源
    /// </summary>
    List<CaptchaResource> RandomGetFontResourceByTypeAndTag(string type, string? tag, int quantity);

    /// <summary>
    /// 随机获取模板资源
    /// </summary>
    List<ResourceMap> RandomGetTemplateResourceByTypeAndTag(CaptchaType type, string? tag, int quantity);

    void ClearAllResources();
    void ClearAllTemplates();

    /// <summary>
    /// 添加资源（用于特殊类型，如Fonts）
    /// </summary>
    void AddResource(CaptchaType type, CaptchaResource resource);

    /// <summary>
    /// 添加模板
    /// </summary>
    void AddTemplate(CaptchaType type, ResourceMap template);

    /// <summary>
    /// 删除资源
    /// </summary>
    CaptchaResource? DeleteResource(CaptchaType type, string id);

    /// <summary>
    /// 删除模板
    /// </summary>
    ResourceMap? DeleteTemplate(CaptchaType type, string id);

    /// <summary>
    /// 列出资源
    /// </summary>
    List<CaptchaResource> ListResourcesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 列出模板
    /// </summary>
    List<ResourceMap> ListTemplatesByTypeAndTag(CaptchaType type, string? tag);

    /// <summary>
    /// 随机获取资源
    /// </summary>
    List<CaptchaResource> RandomGetResourceByTypeAndTag(CaptchaType type, string? tag, int quantity);

    /// <summary>
    /// 随机获取模板
    /// </summary>
    List<ResourceMap> RandomGetTemplateByTypeAndTag(CaptchaType type, string? tag, int quantity);

    /// <summary>
    /// 添加资源（用于特殊类型，如Fonts）
    /// </summary>
    void AddResource(string type, CaptchaResource resource);

    /// <summary>
    /// 删除资源（用于特殊类型，如Fonts）
    /// </summary>
    CaptchaResource? DeleteResource(string type, string id);

    /// <summary>
    /// 列出指定类型和标签的资源（用于特殊类型，如Fonts）
    /// </summary>
    List<CaptchaResource> ListResourcesByTypeAndTag(string type, string? tag);
}
