namespace Tianai.Captcha.Core.Common;

/// <summary>
/// 生成参数的预定义键 (对应 Java ParamKeyEnum)
/// </summary>
public static class ParamKeyEnum
{
    /// <summary>
    /// 点选验证码参与校验的数量
    /// </summary>
    public static readonly ParamKey<int> ClickCheckClickCount = new("checkClickCount");

    /// <summary>
    /// 点选验证码干扰数量
    /// </summary>
    public static readonly ParamKey<int> ClickInterferenceCount = new("interferenceCount");

    /// <summary>
    /// 读取字体时，可指定字体 TAG，可用于给不同的验证码指定不同的字体包
    /// </summary>
    public static readonly ParamKey<string> FontTag = new("fontTag");

    /// <summary>
    /// 容错值
    /// </summary>
    public static readonly ParamKey<float> Tolerant = new("tolerant");

    /// <summary>
    /// 验证码 ID，内部使用
    /// </summary>
    public static readonly ParamKey<string> Id = new("_id");
}
