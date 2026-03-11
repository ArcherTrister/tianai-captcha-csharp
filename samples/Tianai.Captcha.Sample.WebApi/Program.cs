using System.Reflection;
using System.Text.Json;
using System.IO;
using Tianai.Captcha.AspNetCore.Extensions;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using EndpointRouteBuilderExtensions = Tianai.Captcha.AspNetCore.Extensions.EndpointRouteBuilderExtensions;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器服务
builder.Services.AddControllers();

var captchaBuilder = builder.Services.AddTianaiCaptcha(options =>
{
    options.InitDefaultResource = true;
    options.Expire[CaptchaType.WordImageClick] = 30000;
})
.AddResourceAssembly(Assembly.GetExecutingAssembly());

// 示例：扫描目录资源（已注释，因为项目中的Resources资源文件类型是embedded）
// var resourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
// captchaBuilder.ScanDirectory(resourceDirectory);

// // 自定义背景图列表
// var bgImages = new[] { "48.jpg", "a.jpg", "b.jpg", "c.jpg", "d.jpg", "e.jpg", "g.jpg", "h.jpg", "i.jpg", "j.jpg" };
// // 注册自定义背景图（对应 Java addResource）
// foreach (var img in bgImages)
// {
//     foreach (var type in allCaptchaType)
//     {
//         captchaBuilder.AddResource(type, new CaptchaResource("embedded", $"Resources.BgImages.{img}"));
//     }
// }

// // 注册自定义滑块模板（对应 Java addTemplate）
// captchaBuilder.AddSliderTemplate("Resources.Templates.Slider.active_1.png");
// captchaBuilder.AddSliderTemplate("Resources.Templates.Slider.fixed_1.png");

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();

// 添加控制器路由
app.MapControllers();

// --- 库标准端点 ---
app.MapCaptchaEndpoints("/api/captcha");

// --- Web SDK 兼容端点 (对应 Java demo 的 /gen 和 /check) ---

var captchaTypes = CaptchaTypeHelper.GetAll().ToArray();

app.Map("/gen", (HttpContext context, IImageCaptchaApplication captchaApp) =>
{
    var typeStr = context.Request.Query["type"].FirstOrDefault();
    CaptchaType type;
    
    if (string.Equals(typeStr, "RANDOM", StringComparison.OrdinalIgnoreCase))
    {
        type = captchaTypes[Random.Shared.Next(captchaTypes.Length)];
    }
    else if (string.IsNullOrEmpty(typeStr))
    {
        type = CaptchaType.Slider;
    }
    else
    {
        type = Enum.Parse<CaptchaType>(typeStr, true);
    }
    
    var result = captchaApp.GenerateCaptcha(type);
    return Results.Json(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
});

app.MapPost("/check", async (HttpContext context, IImageCaptchaApplication captchaApp) =>
{
    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var body = await JsonSerializer.DeserializeAsync<CheckRequest>(context.Request.Body, jsonOptions);

    if (body == null || string.IsNullOrEmpty(body.Id))
        return Results.Json(ApiResponse<object>.OfCheckError("id is required"), jsonOptions);

    var trackData = body.Data;
    if (trackData == null)
        return Results.Json(ApiResponse<object>.OfCheckError("data is required"), jsonOptions);

    var track = new ImageCaptchaTrack
    {
        BgImageWidth = trackData.BgImageWidth,
        BgImageHeight = trackData.BgImageHeight,
        TemplateImageWidth = trackData.TemplateImageWidth,
        TemplateImageHeight = trackData.TemplateImageHeight,
        StartTime = trackData.StartTime,
        StopTime = trackData.StopTime,
        Tracks = trackData.Tracks ?? new List<ImageCaptchaTrack.Track>()
    };

    var result = captchaApp.Matching(body.Id, track);
    if (result.IsSuccess())
    {
        // SDK expects {code:200, data:{id:"xxx"}} on success
        var successData = new Dictionary<string, object> { ["id"] = body.Id };
        return Results.Json(ApiResponse<object>.OfSuccess(successData), jsonOptions);
    }

    return Results.Json(result, jsonOptions);
});

app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();

// --- Request DTOs for /check endpoint ---

record CheckRequest(string? Id, CheckTrackData? Data);

record CheckTrackData(
    int BgImageWidth,
    int BgImageHeight,
    int TemplateImageWidth,
    int TemplateImageHeight,
    long StartTime,
    long StopTime,
    List<ImageCaptchaTrack.Track>? Tracks);
