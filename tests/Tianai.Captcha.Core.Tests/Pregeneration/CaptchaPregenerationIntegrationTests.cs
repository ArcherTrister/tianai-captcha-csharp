using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Pregeneration;
using Tianai.Captcha.Core.Pregeneration.Impl;
using Tianai.Captcha.Core.Resource;
using Tianai.Captcha.Core.Validator;

namespace Tianai.Captcha.Core.Tests.Pregeneration;

public class CaptchaPregenerationIntegrationTests
{
    private IServiceProvider _serviceProvider;
    private IImageCaptchaApplication _captchaApplication;
    private ICaptchaPregenerationPool _pregenerationPool;
    
    public CaptchaPregenerationIntegrationTests()
    {
        // 配置服务
        var services = new ServiceCollection();
        
        // 配置验证码选项，启用预生成池
        services.Configure<ImageCaptchaOptions>(options =>
        {
            options.PregenerationPoolEnabled = true;
            options.PregenerationPoolMaxCapacity = 10;
            options.PregenerationPoolMinThreshold = 5;
            options.PregenerationPoolCheckIntervalMs = 1000;
        });
        
        // 注册必要的服务
        services.AddSingleton<ICaptchaPregenerationPool>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ImageCaptchaOptions>>().Value;
            return new MemoryCaptchaPregenerationPool(
                options.PregenerationPoolMaxCapacity,
                options.PregenerationPoolMinThreshold);
        });
        
        // 注册其他必要的服务（这里使用模拟实现）
        services.AddSingleton<IImageCaptchaApplication>(sp =>
        {
            // 创建一个模拟的验证码应用
            // 实际测试中应该使用真实的实现
            // 这里为了简化测试，使用一个基本的实现
            var options = sp.GetRequiredService<IOptions<ImageCaptchaOptions>>().Value;
            var pregenerationPool = sp.GetRequiredService<ICaptchaPregenerationPool>();
            
            // 这里应该使用真实的实现，但为了测试，我们创建一个简单的实现
            // 实际项目中，应该使用完整的依赖注入配置
            return new MockImageCaptchaApplication(pregenerationPool);
        });
        
        _serviceProvider = services.BuildServiceProvider();
        _captchaApplication = _serviceProvider.GetRequiredService<IImageCaptchaApplication>();
        _pregenerationPool = _serviceProvider.GetRequiredService<ICaptchaPregenerationPool>();
    }
    
    [Fact]
    public void TestGetCaptchaFromPool()
    {
        // 向池中添加一个验证码
        var testCaptcha = CreateTestCaptcha();
        _pregenerationPool.AddCaptcha(testCaptcha);
        
        // 从应用获取验证码
        var response = _captchaApplication.GenerateCaptcha(CaptchaType.Slider);
        
        // 验证响应
        Assert.True(response.IsSuccess());
        Assert.NotNull(response.Data);
        Assert.Equal(testCaptcha.Id, response.Data.Id);
        
        // 检查池是否为空
        Assert.Equal(0, _pregenerationPool.GetCurrentCount());
    }
    
    [Fact]
    public void TestFallbackToRealTimeGeneration()
    {
        // 确保池为空
        _pregenerationPool.Clear();
        
        // 从应用获取验证码
        var response = _captchaApplication.GenerateCaptcha(CaptchaType.Slider);
        
        // 验证响应
        Assert.True(response.IsSuccess());
        Assert.NotNull(response.Data);
        Assert.False(string.IsNullOrEmpty(response.Data.Id));
    }
    
    [Fact]
    public void TestPoolRefill()
    {
        // 确保池为空
        _pregenerationPool.Clear();
        
        // 检查是否需要补充
        Assert.True(_pregenerationPool.NeedRefill());
        
        // 计算需要补充的数量
        var refillCount = _pregenerationPool.GetRefillCount();
        Assert.Equal(10, refillCount);
        
        // 手动补充验证码
        for (int i = 0; i < refillCount; i++)
        {
            var captcha = CreateTestCaptcha($"pregenerated-{i}");
            _pregenerationPool.AddCaptcha(captcha);
        }
        
        // 检查是否不需要补充
        Assert.False(_pregenerationPool.NeedRefill());
        
        // 检查池容量
        Assert.Equal(10, _pregenerationPool.GetCurrentCount());
    }
    
    private PregeneratedCaptcha CreateTestCaptcha(string id = "test")
    {
        return new PregeneratedCaptcha
        {
            Id = id,
            Type = CaptchaType.Slider,
            Response = new ImageCaptchaResponse
            {
                Id = id,
                Type = "SLIDER",
                BackgroundImage = "base64-image",
                TemplateImage = "base64-template"
            },
            ValidData = new AnyMap(),
            GeneratedTime = DateTime.Now,
            ExpireTime = DateTime.Now.AddMinutes(5)
        };
    }
    
    // 模拟的验证码应用实现
    private class MockImageCaptchaApplication : IImageCaptchaApplication
    {
        private readonly ICaptchaPregenerationPool _pregenerationPool;
        
        public MockImageCaptchaApplication(ICaptchaPregenerationPool pregenerationPool)
        {
            _pregenerationPool = pregenerationPool;
        }
        
        public ApiResponse<ImageCaptchaResponse> GenerateCaptcha()
        {
            // 优先从预生成池获取
            if (_pregenerationPool != null)
            {
                var pregeneratedCaptcha = _pregenerationPool.GetCaptcha();
                if (pregeneratedCaptcha != null && !pregeneratedCaptcha.IsExpired())
                {
                    return ApiResponse<ImageCaptchaResponse>.OfSuccess(pregeneratedCaptcha.Response);
                }
            }
            
            // 回退到实时生成
            var id = "real-time-" + Guid.NewGuid().ToString("N");
            var response = new ImageCaptchaResponse
            {
                Id = id,
                Type = "SLIDER",
                BackgroundImage = "base64-image",
                TemplateImage = "base64-template"
            };
            
            return ApiResponse<ImageCaptchaResponse>.OfSuccess(response);
        }
        
        // 实现其他接口方法（返回默认值）
        public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type) => GenerateCaptcha();
        public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaImageType captchaImageType) => GenerateCaptcha();
        public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type, CaptchaImageType captchaImageType) => GenerateCaptcha();
        public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(GenerateParam param) => GenerateCaptcha();
        public ApiResponse<object> Matching(string id, MatchParam matchParam) => ApiResponse<object>.OfSuccess(true);
        public ApiResponse<object> Matching(string id, ImageCaptchaTrack track) => ApiResponse<object>.OfSuccess(true);
        public bool Matching(string id, float percentage) => true;
        public string? GetCaptchaTypeById(string id) => "SLIDER";
        public ApiResponse<object> VerifySecondaryToken(string token) => ApiResponse<object>.OfSuccess(new { success = true });
        public IImageCaptchaResourceManager GetImageCaptchaResourceManager() => throw new NotImplementedException();
        public IImageCaptchaValidator GetImageCaptchaValidator() => throw new NotImplementedException();
        public IImageCaptchaGenerator GetImageCaptchaGenerator() => throw new NotImplementedException();
        public ICacheStore GetCacheStore() => throw new NotImplementedException();
        public ICaptchaInterceptor GetCaptchaInterceptor() => throw new NotImplementedException();
        public void Dispose() { }
    }
}
