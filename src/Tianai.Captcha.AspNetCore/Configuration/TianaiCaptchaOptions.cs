using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.AspNetCore.Configuration;

public class TianaiCaptchaOptions : ImageCaptchaOptions
{
    /// <summary>
    /// 自定义背景图片资源列表 (对应 Java addResource)
    /// </summary>
    public List<(CaptchaType Type, CaptchaResource Resource)> CustomBackgroundResources { get; }

    /// <summary>
    /// 自定义模板资源列表 (对应 Java addTemplate)
    /// </summary>
    public List<(CaptchaType Type, ResourceMap Template)> CustomTemplateResources { get; }

    /// <summary>
    /// 自定义字体资源列表
    /// </summary>
    public List<(string Type, CaptchaResource Resource)> CustomFontResources { get; }

    /// <summary>
    /// API 端点前缀
    /// </summary>
    public string ApiEndpointPrefix { get; set; } = "/api/captcha";

    /// <summary>
    /// 验证码生成端点路径
    /// </summary>
    public string GenerateEndpoint { get; set; } = "/generate";

    /// <summary>
    /// 验证码验证端点路径
    /// </summary>
    public string ValidateEndpoint { get; set; } = "/validate";

    /// <summary>
    /// 二次验证端点路径
    /// </summary>
    public string SecondaryVerifyEndpoint { get; set; } = "/verify-secondary";

    /// <summary>
    /// 构造函数
    /// </summary>
    public TianaiCaptchaOptions()
    {
        CustomBackgroundResources = new();
        CustomTemplateResources = new();
        CustomFontResources = new();
        Expire = new Dictionary<CaptchaType, long>();
    }
}
