# Tianai.Captcha 使用说明文档

## 1. 项目介绍

Tianai.Captcha 是一个功能强大的验证码库，支持多种验证码类型，包括滑块验证码、旋转验证码、拼接验证码和文字点击验证码。该库提供了灵活的配置选项和易于使用的 API，可用于 ASP.NET Core 应用程序中防止自动化攻击和恶意请求。

### 支持的验证码类型

- **滑块验证码 (Slider)**：用户需要拖动滑块到正确位置
- **旋转验证码 (Rotate)**：用户需要旋转图片到正确方向
- **拼接验证码 (Concat)**：用户需要选择正确的图片拼接
- **文字点击验证码 (WordImageClick)**：用户需要点击图片中指定的文字

## 2. 快速开始

### 2.1 安装依赖

```bash
# 使用 NuGet 安装核心库
Install-Package Tianai.Captcha.Core

# 安装 ASP.NET Core 集成包（可选）
Install-Package Tianai.Captcha.AspNetCore
```

### 2.2 在 ASP.NET Core 中使用

#### 2.2.1 配置服务

在 `Program.cs` 中添加验证码服务：

```csharp
using Tianai.Captcha.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加验证码服务
builder.Services.AddTianaiCaptcha();

var app = builder.Build();

// 注册验证码端点
app.MapTianaiCaptcha();

app.Run();
```

#### 2.2.2 基本使用

```csharp
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;

// 注入 IImageCaptchaApplication
private readonly IImageCaptchaApplication _captchaApplication;

public YourController(IImageCaptchaApplication captchaApplication)
{
    _captchaApplication = captchaApplication;
}

// 生成验证码
[HttpGet("generate")]
public IActionResult Generate()
{
    var result = _captchaApplication.GenerateCaptcha(CaptchaType.Slider);
    return Ok(result);
}

// 验证验证码
[HttpPost("validate")]
public IActionResult Validate([FromBody] ValidateRequest request)
{
    var result = _captchaApplication.Matching(request.Id, request.Percentage);
    return Ok(result);
}
```

## 3. 核心功能

### 3.1 生成验证码

```csharp
// 生成默认类型的验证码
var result1 = _captchaApplication.GenerateCaptcha();

// 生成指定类型的验证码
var result2 = _captchaApplication.GenerateCaptcha(CaptchaType.Slider);

// 生成指定图片类型的验证码
var result3 = _captchaApplication.GenerateCaptcha(CaptchaImageType.WebP);

// 生成指定类型和图片类型的验证码
var result4 = _captchaApplication.GenerateCaptcha(CaptchaType.Rotate, CaptchaImageType.JpegPng);

// 使用自定义参数生成验证码
var param = new GenerateParam
{
    CaptchaType = CaptchaType.Slider,
    ImageType = CaptchaImageType.WebP,
    // 其他参数...
};
var result5 = _captchaApplication.GenerateCaptcha(param);
```

### 3.2 验证验证码

```csharp
// 使用 MatchParam 验证
var matchParam = new MatchParam
{
    Type = TrackTypes.Move,
    // 其他参数...
};
var result1 = _captchaApplication.Matching(captchaId, matchParam);

// 使用 ImageCaptchaTrack 验证
var track = new ImageCaptchaTrack
{
    // 轨迹数据...
};
var result2 = _captchaApplication.Matching(captchaId, track);

// 使用百分比验证（适用于滑块验证码）
var result3 = _captchaApplication.Matching(captchaId, 0.9f); // 90% 匹配度
```

### 3.3 验证码类型管理

```csharp
// 获取验证码类型
string? captchaType = _captchaApplication.GetCaptchaTypeById(captchaId);
```

## 4. 配置选项

### 4.1 基本配置

```csharp
builder.Services.AddTianaiCaptcha(options =>
{
    // 是否初始化默认资源
    options.InitDefaultResource = true;
    
    // 缓存键前缀
    options.Prefix = "tianai:captcha";
    
    // 各验证码类型的过期时间(毫秒)
    options.Expire[CaptchaType.Slider] = 300000; // 5分钟
    options.Expire[CaptchaType.Rotate] = 300000; // 5分钟
    options.Expire[CaptchaType.Concat] = 300000; // 5分钟
    options.Expire[CaptchaType.WordImageClick] = 300000; // 5分钟
    
    // 启用本地缓存
    options.LocalCacheEnabled = true;
    options.LocalCacheSize = 50;
    
    // 启用预生成池
    options.PregenerationPoolEnabled = true;
    options.PregenerationPoolMaxCapacity = 100;
    options.PregenerationPoolMinThreshold = 50;
});
```

### 4.2 自定义资源配置

```csharp
builder.Services.AddTianaiCaptcha(options =>
{
    // 初始化默认资源
    options.InitDefaultResource = true;
    
    // 添加自定义字体路径
    options.FontPath = new List<string>
    {
        "embedded:YourAssembly.Resources.Fonts.font.ttf"
    };
});

// 通过扩展方法添加资源
builder.Services.AddTianaiCaptcha()
    .AddResourceAssembly(Assembly.GetExecutingAssembly())
    .ScanDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Resources"));
```

## 5. 资源管理

### 5.1 自定义资源

您可以在项目中添加自定义的背景图片和模板图片：

1. 创建 `Resources` 目录
2. 在 `Resources` 下创建 `BgImages/All` 目录存放背景图片
3. 在 `Resources` 下创建 `Templates` 目录，按验证码类型存放模板图片：
   - `Templates/Slider`：滑块验证码模板
   - `Templates/Rotate`：旋转验证码模板

### 5.2 内置资源

库中已内置了一些默认资源，位于 `Tianai.Captcha.Core/Resources` 目录。

## 6. 预生成验证码

为了提高性能，您可以启用验证码预生成功能：

```csharp
builder.Services.AddTianaiCaptcha(options =>
{
    // 启用预生成池
    options.PregenerationPoolEnabled = true;
    
    // 预生成池的最大容量
    options.PregenerationPoolMaxCapacity = 100;
    
    // 预生成池的最低阈值，当低于此值时会触发批量生成
    options.PregenerationPoolMinThreshold = 50;
    
    // 预生成池检查的时间间隔（毫秒）
    options.PregenerationPoolCheckIntervalMs = 30000;
});
```

## 7. 示例项目

项目包含一个示例 Web API 项目 `Tianai.Captcha.Sample.WebApi`，展示了如何在 ASP.NET Core 应用中使用该验证码库。

### 7.1 运行示例

1. 打开 `tianai-captcha.sln` 解决方案
2. 设置 `Tianai.Captcha.Sample.WebApi` 为启动项目
3. 运行项目
4. 访问 `https://localhost:5000/swagger` 查看 API 文档

### 7.2 示例 API

- `GET /api/resources/bg-images`：获取背景图片列表
- `GET /api/resources/templates`：获取模板图片列表

## 8. 常见问题

### 8.1 验证码不显示

- 检查资源文件是否正确添加
- 确认验证码生成调用是否正确
- 检查网络请求是否成功

### 8.2 验证失败

- 检查验证码 ID 是否正确
- 确认验证参数是否符合要求
- 检查验证码是否已过期

### 8.3 性能问题

- 启用预生成功能
- 优化资源文件大小
- 考虑使用分布式缓存

## 9. 高级功能

### 9.1 自定义验证码生成器

您可以实现 `IImageCaptchaGenerator` 接口来自定义验证码生成逻辑。

### 9.2 自定义资源提供者

您可以实现 `IResourceProvider` 接口来自定义资源加载逻辑。

### 9.3 自定义缓存存储

您可以实现 `ICacheStore` 接口来自定义缓存逻辑，例如使用 Redis 作为缓存存储。

## 10. 版本历史

- **v1.0.0**：初始版本，支持滑块验证码
- **v1.1.0**：添加旋转验证码支持
- **v1.2.0**：添加拼接验证码和文字点击验证码支持
- **v1.3.0**：添加预生成验证码功能
- **v1.4.0**：优化性能和资源管理

## 11. 贡献指南

欢迎提交 Issue 和 Pull Request 来改进这个项目。

## 12. 许可证

本项目采用 MIT 许可证。