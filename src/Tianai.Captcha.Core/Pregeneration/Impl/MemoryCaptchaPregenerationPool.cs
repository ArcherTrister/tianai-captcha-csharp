using System.Collections.Concurrent;
using System.Diagnostics;
using Tianai.Captcha.Core.Pregeneration;

namespace Tianai.Captcha.Core.Pregeneration.Impl;

public class MemoryCaptchaPregenerationPool : ICaptchaPregenerationPool
{
    private readonly ConcurrentQueue<PregeneratedCaptcha> _captchaQueue;
    private readonly int _maxCapacity;
    private readonly int _minThreshold;
    private readonly object _lock = new object();
    private DateTime? _lastRefillTime;
    private DateTime? _lastGetTime;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    public MemoryCaptchaPregenerationPool(int maxCapacity, int minThreshold)
    {
        _maxCapacity = maxCapacity > 0 ? maxCapacity : 100;
        _minThreshold = minThreshold > 0 && minThreshold < _maxCapacity ? minThreshold : _maxCapacity / 2;
        _captchaQueue = new ConcurrentQueue<PregeneratedCaptcha>();
    }
    
    public void AddCaptcha(PregeneratedCaptcha captcha)
    {
        if (captcha == null)
            return;
        
        lock (_lock)
        {
            if (_captchaQueue.Count >= _maxCapacity)
            {
                // 池已满，移除最早的验证码
                _captchaQueue.TryDequeue(out _);
                Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 池已满，移除最早的验证码，当前容量: {_captchaQueue.Count}");
            }
            
            _captchaQueue.Enqueue(captcha);
            _lastRefillTime = DateTime.Now;
            Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 添加验证码 {captcha.Id}，当前容量: {_captchaQueue.Count}");
        }
    }
    
    public PregeneratedCaptcha? GetCaptcha()
    {
        CleanExpiredCaptchas();
        
        if (_captchaQueue.TryDequeue(out var captcha))
        {
            if (!captcha.IsExpired())
            {
                _lastGetTime = DateTime.Now;
                Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 获取验证码 {captcha.Id}，剩余容量: {_captchaQueue.Count}");
                return captcha;
            }
            else
            {
                Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 获取到过期验证码 {captcha.Id}");
            }
        }
        else
        {
            Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 池为空，无法获取验证码");
        }
        
        return null;
    }
    
    public int GetCurrentCount()
    {
        CleanExpiredCaptchas();
        return _captchaQueue.Count;
    }
    
    public void CleanExpiredCaptchas()
    {
        lock (_lock)
        {
            var expiredCount = 0;
            while (_captchaQueue.TryPeek(out var captcha) && captcha.IsExpired())
            {
                _captchaQueue.TryDequeue(out _);
                expiredCount++;
                Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 清理过期验证码 {captcha.Id}");
            }
            if (expiredCount > 0)
            {
                Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 共清理 {expiredCount} 个过期验证码，剩余容量: {_captchaQueue.Count}");
            }
        }
    }
    
    public void Clear()
    {
        lock (_lock)
        {
            while (_captchaQueue.TryDequeue(out _))
            {
                // 清空队列
            }
        }
    }
    
    public int MaxCapacity => _maxCapacity;
    
    public int MinThreshold => _minThreshold;
    
    public bool NeedRefill()
    {
        var currentCount = GetCurrentCount();
        var needRefill = currentCount < _minThreshold;
        Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 检查是否需要补充验证码，当前容量: {currentCount}, 最小阈值: {_minThreshold}, 需要补充: {needRefill}");
        return needRefill;
    }
    
    public int GetRefillCount()
    {
        var currentCount = GetCurrentCount();
        var refillCount = _maxCapacity - currentCount;
        var result = Math.Max(0, refillCount);
        Debug.WriteLine($"[{_stopwatch.ElapsedMilliseconds}ms] 计算需要补充的验证码数量，当前容量: {currentCount}, 最大容量: {_maxCapacity}, 需要补充: {result}");
        return result;
    }
    
    public CaptchaPregenerationPoolStatus GetStatus()
    {
        var currentCount = GetCurrentCount();
        var fillRate = _maxCapacity > 0 ? (double)currentCount / _maxCapacity : 0;
        var needRefill = NeedRefill();
        var refillCount = GetRefillCount();
        
        return new CaptchaPregenerationPoolStatus
        {
            CurrentCount = currentCount,
            MaxCapacity = _maxCapacity,
            MinThreshold = _minThreshold,
            FillRate = fillRate,
            NeedRefill = needRefill,
            RefillCount = refillCount,
            LastRefillTime = _lastRefillTime,
            LastGetTime = _lastGetTime
        };
    }
}
