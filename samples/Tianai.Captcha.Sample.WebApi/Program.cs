using System.Reflection;
using System.Text.Json;
using System.IO;
using Tianai.Captcha.AspNetCore.Extensions;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器服务
builder.Services.AddControllers();

var captchaBuilder = builder.Services.AddTianaiCaptcha(options =>
{
    options.InitDefaultResource = true;
    options.Expire[CaptchaType.WordImageClick] = 30000;
    // 使用默认的 Web SDK 兼容端点
})
.ScanAssembly(Assembly.GetExecutingAssembly())
// 扫描 wwwroot/CaptchaResources 目录以加载验证码资源
.ScanDirectory(Path.Combine(builder.Environment.WebRootPath, "CaptchaResources"));

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();

// 添加控制器路由
app.MapControllers();

// 映射验证码端点
app.MapCaptchaEndpoints();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();
