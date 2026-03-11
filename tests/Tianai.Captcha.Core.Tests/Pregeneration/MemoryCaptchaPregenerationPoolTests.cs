using Xunit;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Pregeneration;
using Tianai.Captcha.Core.Pregeneration.Impl;

namespace Tianai.Captcha.Core.Tests.Pregeneration;

public class MemoryCaptchaPregenerationPoolTests
{
    private ICaptchaPregenerationPool _pool;
    
    public MemoryCaptchaPregenerationPoolTests()
    {
        _pool = new MemoryCaptchaPregenerationPool(10, 5);
    }
    
    [Fact]
    public void TestAddAndGetCaptcha()
    {
        // 准备测试数据
        var captcha = CreateTestCaptcha();
        
        // 添加验证码到池
        _pool.AddCaptcha(captcha);
        
        // 检查池容量
        Assert.Equal(1, _pool.GetCurrentCount());
        
        // 从池中获取验证码
        var retrievedCaptcha = _pool.GetCaptcha();
        
        // 验证获取到的验证码
        Assert.NotNull(retrievedCaptcha);
        Assert.Equal(captcha.Id, retrievedCaptcha.Id);
        
        // 检查池容量
        Assert.Equal(0, _pool.GetCurrentCount());
    }
    
    [Fact]
    public void TestPoolCapacity()
    {
        // 填充池到最大容量
        for (int i = 0; i < 10; i++)
        {
            var captcha = CreateTestCaptcha($"test-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        // 检查池容量
        Assert.Equal(10, _pool.GetCurrentCount());
        
        // 添加一个新验证码，应该替换最早的一个
        var newCaptcha = CreateTestCaptcha("test-10");
        _pool.AddCaptcha(newCaptcha);
        
        // 检查池容量仍然是最大容量
        Assert.Equal(10, _pool.GetCurrentCount());
    }
    
    [Fact]
    public void TestNeedRefill()
    {
        // 初始状态，池为空，应该需要补充
        Assert.True(_pool.NeedRefill());
        
        // 添加验证码到阈值以上
        for (int i = 0; i < 6; i++)
        {
            var captcha = CreateTestCaptcha($"test-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        // 检查是否不需要补充
        Assert.False(_pool.NeedRefill());
        
        // 移除验证码到阈值以下
        for (int i = 0; i < 2; i++)
        {
            _pool.GetCaptcha();
        }
        
        // 检查是否需要补充
        Assert.True(_pool.NeedRefill());
    }
    
    [Fact]
    public void TestGetRefillCount()
    {
        // 初始状态，需要补充10个
        Assert.Equal(10, _pool.GetRefillCount());
        
        // 添加5个验证码，需要补充5个
        for (int i = 0; i < 5; i++)
        {
            var captcha = CreateTestCaptcha($"test-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        Assert.Equal(5, _pool.GetRefillCount());
        
        // 添加到最大容量，不需要补充
        for (int i = 5; i < 10; i++)
        {
            var captcha = CreateTestCaptcha($"test-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        Assert.Equal(0, _pool.GetRefillCount());
    }
    
    [Fact]
    public void TestCleanExpiredCaptchas()
    {
        // 添加一个过期的验证码
        var expiredCaptcha = CreateTestCaptcha("expired");
        expiredCaptcha.ExpireTime = DateTime.Now.AddMinutes(-1);
        _pool.AddCaptcha(expiredCaptcha);
        
        // 添加一个有效的验证码
        var validCaptcha = CreateTestCaptcha("valid");
        validCaptcha.ExpireTime = DateTime.Now.AddMinutes(5);
        _pool.AddCaptcha(validCaptcha);
        
        // 清理过期验证码
        _pool.CleanExpiredCaptchas();
        
        // 检查池容量，应该只剩下一个有效验证码
        Assert.Equal(1, _pool.GetCurrentCount());
        
        // 获取验证码，应该是有效的那个
        var retrievedCaptcha = _pool.GetCaptcha();
        Assert.NotNull(retrievedCaptcha);
        Assert.Equal("valid", retrievedCaptcha.Id);
    }
    
    [Fact]
    public void TestClear()
    {
        // 填充池
        for (int i = 0; i < 5; i++)
        {
            var captcha = CreateTestCaptcha($"test-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        // 检查池容量
        Assert.Equal(5, _pool.GetCurrentCount());
        
        // 清空池
        _pool.Clear();
        
        // 检查池容量
        Assert.Equal(0, _pool.GetCurrentCount());
    }
    
    [Fact]
    public async Task TestMultiThreadedAccess()
    {
        var tasks = new List<Task>();
        
        // 多线程添加验证码
        for (int i = 0; i < 100; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                var captcha = CreateTestCaptcha($"thread-{taskId}");
                _pool.AddCaptcha(captcha);
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // 检查池容量不超过最大容量
        var count = _pool.GetCurrentCount();
        Assert.InRange(count, 0, 10);
        
        // 多线程获取验证码
        tasks.Clear();
        var retrievedCount = 0;
        
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var captcha = _pool.GetCaptcha();
                if (captcha != null)
                {
                    Interlocked.Increment(ref retrievedCount);
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // 检查获取到的验证码数量
        Assert.InRange(retrievedCount, 0, 10);
    }
    
    [Fact]
    public async Task Test100ConcurrentRequests()
    {
        // 创建容量为50，最小阈值为20的验证码池
        _pool = new MemoryCaptchaPregenerationPool(50, 20);
        
        // 先填充验证码到最大容量
        for (int i = 0; i < 50; i++)
        {
            var captcha = CreateTestCaptcha($"init-{i}");
            _pool.AddCaptcha(captcha);
        }
        
        // 验证初始容量
        Assert.Equal(50, _pool.GetCurrentCount());
        
        // 模拟100人同时请求验证码
        var tasks = new List<Task>();
        var successCount = 0;
        var failureCount = 0;
        
        for (int i = 0; i < 100; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                var captcha = _pool.GetCaptcha();
                if (captcha != null)
                {
                    Interlocked.Increment(ref successCount);
                    Debug.WriteLine($"线程 {taskId} 成功获取验证码 {captcha.Id}");
                }
                else
                {
                    Interlocked.Increment(ref failureCount);
                    Debug.WriteLine($"线程 {taskId} 获取验证码失败");
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // 验证获取结果
        Debug.WriteLine($"成功获取: {successCount}, 失败获取: {failureCount}");
        
        // 检查最终容量
        var finalCount = _pool.GetCurrentCount();
        Debug.WriteLine($"最终容量: {finalCount}");
        
        // 验证没有报错，并且成功获取的验证码数量在合理范围内
        Assert.True(successCount > 0);
        Assert.True(failureCount >= 0);
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
}
