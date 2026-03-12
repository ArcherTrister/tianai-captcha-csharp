using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Pregeneration;

public interface ICaptchaPregenerationPool
{
    /// <summary>
    /// 添加预生成的验证码到池中
    /// </summary>
    /// <param name="captcha">预生成的验证码</param>
    void AddCaptcha(PregeneratedCaptcha captcha);
    
    /// <summary>
    /// 从池中获取一个验证码
    /// // todo: 这是需要删除
    /// </summary>
    /// <returns>预生成的验证码，如果池为空则返回null</returns>
    PregeneratedCaptcha? GetCaptcha();
    
    /// <summary>
    /// 从池中获取指定类型的验证码
    /// </summary>
    /// <param name="type">验证码类型</param>
    /// <returns>预生成的验证码，如果池为空或没有指定类型的验证码则返回null</returns>
    PregeneratedCaptcha? GetCaptcha(CaptchaType type);
    
    /// <summary>
    /// 检查池水位
    /// </summary>
    /// <returns>当前池中的验证码数量</returns>
    int GetCurrentCount();
    
    /// <summary>
    /// 清理过期的验证码
    /// </summary>
    void CleanExpiredCaptchas();
    
    /// <summary>
    /// 清空池
    /// </summary>
    void Clear();
    
    /// <summary>
    /// 获取池的最大容量
    /// </summary>
    int MaxCapacity { get; }
    
    /// <summary>
    /// 获取池的最低阈值
    /// </summary>
    int MinThreshold { get; }
    
    /// <summary>
    /// 检查是否需要补充验证码
    /// </summary>
    /// <returns>如果当前数量低于阈值则返回true</returns>
    bool NeedRefill();
    
    /// <summary>
    /// 获取需要补充的验证码数量
    /// </summary>
    /// <returns>需要补充的验证码数量</returns>
    int GetRefillCount();
    
    /// <summary>
    /// 获取预生成池的状态信息
    /// </summary>
    /// <returns>预生成池状态</returns>
    CaptchaPregenerationPoolStatus GetStatus();
}

/// <summary>
/// 预生成池状态
/// </summary>
public class CaptchaPregenerationPoolStatus
{
    /// <summary>
    /// 当前池中的验证码数量
    /// </summary>
    public int CurrentCount { get; set; }
    
    /// <summary>
    /// 池的最大容量
    /// </summary>
    public int MaxCapacity { get; set; }
    
    /// <summary>
    /// 池的最低阈值
    /// </summary>
    public int MinThreshold { get; set; }
    
    /// <summary>
    /// 填充率（当前数量/最大容量）
    /// </summary>
    public double FillRate { get; set; }
    
    /// <summary>
    /// 是否需要补充验证码
    /// </summary>
    public bool NeedRefill { get; set; }
    
    /// <summary>
    /// 需要补充的验证码数量
    /// </summary>
    public int RefillCount { get; set; }
    
    /// <summary>
    /// 最后一次填充时间
    /// </summary>
    public DateTime? LastRefillTime { get; set; }
    
    /// <summary>
    /// 最后一次获取验证码时间
    /// </summary>
    public DateTime? LastGetTime { get; set; }
}
