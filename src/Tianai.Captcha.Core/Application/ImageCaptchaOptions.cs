using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Application;

public class ImageCaptchaOptions
{
    /// <summary>
    /// 缓存 key 前缀
    /// </summary>
    public string Prefix { get; set; } = "captcha";
    
    /// <summary>
    /// 是否初始化默认资源 (对应 Java initDefaultResource，默认 false)
    /// </summary>
    public bool InitDefaultResource { get; set; } = true;
    
    /// <summary>
    /// 默认资源前缀
    /// </summary>
    public string DefaultResourcePrefix { get; set; } = "Tianai.Captcha.Core.Resources";
    
    /// <summary>
    /// 字体包路径 (格式: "type:path?tag=xxx")
    /// </summary>
    public List<string>? FontPath { get; set; }
    
    /// <summary>
    /// 默认验证码过期时间(毫秒)
    /// </summary>
    private readonly long DefaultCaptchaExpire = 2000;

    /// <summary>
    /// 各验证码类型的过期时间(毫秒)
    /// </summary>
    public Dictionary<CaptchaType, long> Expire { get; set; } = new();

    // ---- 本地预生成缓存配置 ----

    public bool LocalCacheEnabled { get; set; }
    public int LocalCacheSize { get; set; } = 20;
    public int LocalCacheWaitTime { get; set; } = 5000;
    public int LocalCachePeriod { get; set; } = 2000;
    public long? LocalCacheExpireTime { get; set; }

    /// <summary>
    /// 缓存键计算时忽略的字段集合
    /// </summary>
    public HashSet<string>? LocalCacheIgnoredCacheFields { get; set; }

    // ---- 验证码预生成池配置 ----

    /// <summary>
    /// 是否启用验证码预生成池
    /// </summary>
    public bool PregenerationPoolEnabled { get; set; } = false;

    /// <summary>
    /// 预生成池的最大容量
    /// </summary>
    public int PregenerationPoolMaxCapacity { get; set; } = 100;

    /// <summary>
    /// 预生成池的最低阈值，当低于此值时会触发批量生成
    /// </summary>
    public int PregenerationPoolMinThreshold { get; set; } = 50;

    /// <summary>
    /// 预生成池检查的时间间隔（毫秒）
    /// </summary>
    public int PregenerationPoolCheckIntervalMs { get; set; } = 30000;

    /// <summary>
    /// 获取指定类型的过期时间(毫秒)，
    /// 先查类型对应值，再查默认值，兜底 20000ms
    /// </summary>
    public long GetExpire(CaptchaType type)
    {
        return Expire.GetValueOrDefault(type, DefaultCaptchaExpire);
    }
}
