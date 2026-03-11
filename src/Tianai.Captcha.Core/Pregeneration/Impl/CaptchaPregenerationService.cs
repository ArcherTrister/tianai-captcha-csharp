using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Pregeneration;

namespace Tianai.Captcha.Core.Pregeneration.Impl;

public class CaptchaPregenerationService : BackgroundService
{
    private readonly IImageCaptchaApplication _captchaApplication;
    private readonly ICaptchaPregenerationPool? _pregenerationPool;
    private readonly ILogger _logger;
    private readonly int _checkIntervalMs;
    private readonly CaptchaType[] _captchaTypes;
    
    public CaptchaPregenerationService(
        IImageCaptchaApplication captchaApplication,
        ICaptchaPregenerationPool? pregenerationPool,
        IOptions<ImageCaptchaOptions> options,
        ILogger<CaptchaPregenerationService>? logger = null)
    {
        _captchaApplication = captchaApplication;
        _pregenerationPool = pregenerationPool;
        _checkIntervalMs = options.Value.PregenerationPoolCheckIntervalMs;
        _logger = logger ?? NullLogger<CaptchaPregenerationService>.Instance;
        
        // 支持的验证码类型
        _captchaTypes = new[] 
        {
            CaptchaType.Slider,
            CaptchaType.Rotate,
            CaptchaType.WordImageClick
        };
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_pregenerationPool == null)
        {
            _logger.LogInformation("验证码预生成池未启用，服务已停止");
            return;
        }
        
        _logger.LogInformation("验证码预生成服务启动");
        
        // 初始填充
        await RefillCaptchasAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkIntervalMs, stoppingToken);
                
                if (_pregenerationPool != null && _pregenerationPool.NeedRefill())
                {
                    await RefillCaptchasAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 任务被取消，正常退出
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证码预生成服务错误");
                // 继续执行，不中断服务
            }
        }
        
        _logger.LogInformation("验证码预生成服务停止");
    }
    
    private async Task RefillCaptchasAsync(CancellationToken stoppingToken)
    {
        if (_pregenerationPool == null)
            return;
            
        var refillCount = _pregenerationPool.GetRefillCount();
        if (refillCount <= 0)
            return;
        
        _logger.LogInformation("开始补充验证码，需要补充 {Count} 个", refillCount);
        
        var generatedCount = 0;
        for (int i = 0; i < refillCount && !stoppingToken.IsCancellationRequested; i++)
        {
            try
            {
                // 随机选择验证码类型
                var randomType = _captchaTypes[new Random().Next(_captchaTypes.Length)];
                var param = new GenerateParam { CaptchaType = randomType };
                
                // 使用不缓存的方式生成验证码
                var (response, validData) = ((DefaultImageCaptchaApplication)_captchaApplication).GenerateCaptchaWithoutCache(param);
                
                if (response != null && response.IsSuccess() && response.Data != null)
                {
                    var captcha = new PregeneratedCaptcha
                    {
                        Id = response.Data.Id,
                        Type = randomType,
                        Response = response.Data,
                        ValidData = validData,
                        GeneratedTime = DateTime.Now,
                        ExpireTime = DateTime.Now.AddMinutes(5) // 5分钟过期
                    };
                    
                    _pregenerationPool?.AddCaptcha(captcha);
                    generatedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成预验证码失败");
            }
        }
        
        _logger.LogInformation("验证码补充完成，生成了 {Count} 个", generatedCount);
    }
}
