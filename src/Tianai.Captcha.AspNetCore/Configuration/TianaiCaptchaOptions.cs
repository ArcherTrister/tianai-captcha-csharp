using System.Reflection;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.AspNetCore.Configuration;

public class TianaiCaptchaOptions : ImageCaptchaOptions
{
    /// <summary>
    /// 二次验证配置
    /// </summary>
    public SecondaryVerificationOptions? Secondary { get; set; }

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
    /// 构造函数
    /// </summary>
    public TianaiCaptchaOptions()
    {
        CustomBackgroundResources = new();
        CustomTemplateResources = new();
        CustomFontResources = new();
    }
}