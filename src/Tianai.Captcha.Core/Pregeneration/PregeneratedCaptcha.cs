using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Pregeneration;

public class PregeneratedCaptcha
{
    /// <summary>
    /// 验证码ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// 验证码类型
    /// </summary>
    public CaptchaType Type { get; set; }
    
    /// <summary>
    /// 验证码响应数据
    /// </summary>
    public ImageCaptchaResponse Response { get; set; }
    
    /// <summary>
    /// 验证码验证数据
    /// </summary>
    public AnyMap ValidData { get; set; }
    
    /// <summary>
    /// 生成时间
    /// </summary>
    public DateTime GeneratedTime { get; set; }
    
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }
    
    /// <summary>
    /// 检查验证码是否过期
    /// </summary>
    /// <returns>如果过期则返回true</returns>
    public bool IsExpired()
    {
        return DateTime.Now > ExpireTime;
    }
    
    /// <summary>
    /// 获取验证码剩余有效时间（毫秒）
    /// </summary>
    /// <returns>剩余有效时间</returns>
    public long GetRemainingTime()
    {
        var remaining = ExpireTime - DateTime.Now;
        return (long)remaining.TotalMilliseconds;
    }
}
