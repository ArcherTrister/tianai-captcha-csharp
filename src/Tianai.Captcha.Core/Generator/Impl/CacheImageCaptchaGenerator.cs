using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Generator.Impl;

/// <summary>
/// 缓存键，支持忽略指定字段
/// </summary>
public sealed class CacheKey : IEquatable<CacheKey>
{
    public GenerateParam GenerateParam { get; }
    private readonly HashSet<string> _ignoredFields;

    public CacheKey(GenerateParam generateParam, IEnumerable<string>? ignoredFields = null)
    {
        GenerateParam = generateParam ?? throw new ArgumentNullException(nameof(generateParam));
        _ignoredFields = ignoredFields != null ? new HashSet<string>(ignoredFields) : new HashSet<string>();
    }

    private Dictionary<string, object?> GetEffectiveFields()
    {
        var map = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var kvp in GenerateParam)
        {
            if (!_ignoredFields.Contains(kvp.Key))
                map[kvp.Key] = kvp.Value;
        }
        return map;
    }

    public bool Equals(CacheKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        var thisFields = GetEffectiveFields();
        var otherFields = other.GetEffectiveFields();

        if (thisFields.Count != otherFields.Count) return false;
        foreach (var kvp in thisFields)
        {
            if (!otherFields.TryGetValue(kvp.Key, out var otherVal))
                return false;
            if (!Equals(kvp.Value, otherVal))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as CacheKey);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in GetEffectiveFields().OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        var fields = GetEffectiveFields();
        return $"CacheKey{{effectiveFields={string.Join(", ", fields.Select(kv => $"{kv.Key}={kv.Value}"))}}}";
    }
}

/// <summary>
/// 验证码缓存生成器 (对应 Java CacheImageCaptchaGenerator)
/// </summary>
public class CacheImageCaptchaGenerator : IImageCaptchaGenerator, IDisposable
{
    private readonly IImageCaptchaGenerator _target;
    private readonly int _size;
    private readonly int _waitTime;
    private readonly int _period;
    private readonly long _expireTime;
    private readonly ConcurrentDictionary<CacheKey, ConcurrentQueue<ImageCaptchaInfo>> _queueMap = new();
    private readonly ConcurrentDictionary<CacheKey, int> _posMap = new();
    private readonly ConcurrentDictionary<CacheKey, long> _lastUpdateMap = new();
    private Timer? _timer;
    private bool _init;
    private bool _disposed;
    private readonly ILogger _logger;

    /// <summary>
    /// 缓存键计算时忽略的字段集合
    /// </summary>
    public HashSet<string> IgnoredCacheFields { get; set; } = new();

    /// <summary>
    /// 缓存中取不到时是否直接生成
    /// </summary>
    public bool RequiredGetCaptcha { get; set; } = true;

    public CacheImageCaptchaGenerator(IImageCaptchaGenerator target, int size = 10,
        int waitTime = 1000, int period = 5000, long? expireTime = null,
        ILogger<CacheImageCaptchaGenerator>? logger = null)
    {
        _target = target;
        _size = size;
        _waitTime = waitTime;
        _period = period;
        // 默认 10 天过期
        _expireTime = expireTime ?? (long)TimeSpan.FromDays(10).TotalMilliseconds;
        _logger = logger ?? NullLogger<CacheImageCaptchaGenerator>.Instance;
    }

    public IImageCaptchaGenerator Init()
    {
        _target.Init();
        if (!_init)
        {
            _timer = new Timer(FillCache, null, 0, _period);
            _init = true;
        }
        return this;
    }

    private void FillCache(object? state)
    {
        foreach (var kvp in _queueMap)
        {
            var cacheKey = kvp.Key;
            var queue = kvp.Value;
            try
            {
                var pos = _posMap.GetOrAdd(cacheKey, 0);
                int addCount = 0;

                while (pos < _size)
                {
                    var generateParam = BeforeGenerateCaptchaImage(cacheKey.GenerateParam);
                    var info = _target.GenerateCaptchaImage(generateParam);
                    if (info != null)
                    {
                        queue.Enqueue(info);
                        addCount++;
                        _posMap.AddOrUpdate(cacheKey, pos + 1, (_, old) => old + 1);
                        pos = _posMap[cacheKey];
                    }
                    else
                    {
                        Sleep();
                        break;
                    }
                }

                if (addCount == 0)
                {
                    // 检测最新更新时间, 超时则清除
                    if (_lastUpdateMap.TryGetValue(cacheKey, out var lastUpdate)
                        && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastUpdate > _expireTime)
                    {
                        _queueMap.TryRemove(cacheKey, out _);
                        _posMap.TryRemove(cacheKey, out _);
                        _lastUpdateMap.TryRemove(cacheKey, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存队列扫描出错");
                _queueMap.TryRemove(cacheKey, out _);
                _posMap.TryRemove(cacheKey, out _);
                _lastUpdateMap.TryRemove(cacheKey, out _);
                Sleep();
            }
        }
    }

    private void Sleep()
    {
        try { Thread.Sleep(_waitTime); } catch { /* ignored */ }
    }

    public ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type)
    {
        var param = new GenerateParam { CaptchaType = type };
        return GenerateCaptchaImage(param);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(CaptchaType type, string bgFormat, string tplFormat)
    {
        var param = GenerateParam.CreateBuilder()
            .Type(type)
            .BackgroundFormatName(bgFormat)
            .TemplateFormatName(tplFormat)
            .Build();
        return GenerateCaptchaImage(param);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(string type)
    {
        var captchaType = Enum.TryParse<CaptchaType>(type, true, out var result) ? result : CaptchaType.Slider;
        var param = new GenerateParam { CaptchaType = captchaType };
        return GenerateCaptchaImage(param);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(string type, string bgFormat, string tplFormat)
    {
        var captchaType = Enum.TryParse<CaptchaType>(type, true, out var result) ? result : CaptchaType.Slider;
        var param = GenerateParam.CreateBuilder()
            .Type(captchaType)
            .BackgroundFormatName(bgFormat)
            .TemplateFormatName(tplFormat)
            .Build();
        return GenerateCaptchaImage(param);
    }

    public ImageCaptchaInfo GenerateCaptchaImage(GenerateParam param)
    {
        var cacheKey = new CacheKey(param, IgnoredCacheFields);

        ImageCaptchaInfo? captchaInfo = null;
        if (_queueMap.TryGetValue(cacheKey, out var queue))
        {
            if (queue.TryDequeue(out captchaInfo))
            {
                _posMap.AddOrUpdate(cacheKey, 0, (_, old) => Math.Max(0, old - 1));
            }
            else
            {
                _logger.LogWarning("验证码缓存不足, genParam: {Param}", param.Type);
            }
        }
        else
        {
            cacheKey = BeforeAddQueue(cacheKey);
            _queueMap.TryAdd(cacheKey, new ConcurrentQueue<ImageCaptchaInfo>());
            _posMap.TryAdd(cacheKey, 0);
        }

        if (captchaInfo == null && RequiredGetCaptcha)
        {
            captchaInfo = _target.GenerateCaptchaImage(param);
        }

        if (captchaInfo != null)
        {
            _lastUpdateMap[cacheKey] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        return captchaInfo!;
    }

    public IImageCaptchaResourceManager GetImageResourceManager() => _target.GetImageResourceManager();
    public void SetImageResourceManager(IImageCaptchaResourceManager manager) => _target.SetImageResourceManager(manager);
    public IImageTransform? GetImageTransform() => _target.GetImageTransform();
    public void SetImageTransform(IImageTransform transform) => _target.SetImageTransform(transform);
    public ICaptchaInterceptor? GetInterceptor() => _target.GetInterceptor();
    public void SetInterceptor(ICaptchaInterceptor interceptor) => _target.SetInterceptor(interceptor);

    /// <summary>
    /// 添加到队列前扩展点
    /// </summary>
    protected virtual CacheKey BeforeAddQueue(CacheKey cacheKey) => cacheKey;

    /// <summary>
    /// 生成验证码前扩展点
    /// </summary>
    protected virtual GenerateParam BeforeGenerateCaptchaImage(GenerateParam param) => param;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
        _queueMap.Clear();
        _posMap.Clear();
        _lastUpdateMap.Clear();
        if (_target is IDisposable d) d.Dispose();
    }
}
