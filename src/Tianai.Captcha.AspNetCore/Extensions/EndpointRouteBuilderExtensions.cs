using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tianai.Captcha.AspNetCore.Configuration;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.AspNetCore.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 映射验证码端点
    /// </summary>
    /// <param name="app">Web 应用</param>
    /// <returns>Web 应用</returns>
    public static WebApplication MapCaptchaEndpoints(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<TianaiCaptchaOptions>>().Value;
        var group = app.MapGroup(options.ApiEndpointPrefix);
        var captchaTypes = CaptchaTypeHelper.GetAll().ToArray();

        group.MapPost(options.GenerateEndpoint, (string type, IImageCaptchaApplication captchaApp) =>
        {
            CaptchaType captchaType;
    
            if (string.Equals(type, "RANDOM", StringComparison.OrdinalIgnoreCase))
            {
                captchaType = captchaTypes[Random.Shared.Next(captchaTypes.Length)];
            }
            else if (string.IsNullOrEmpty(type))
            {
                captchaType = CaptchaType.Slider;
            }
            else
            {
                captchaType = Enum.Parse<CaptchaType>(type, true);
            }
    
            var result = captchaApp.GenerateCaptcha(captchaType);

            return Results.Json(result);
        });

        group.MapPost(options.ValidateEndpoint, (ValidateRequest request, IImageCaptchaApplication captchaApp) =>
        {
            if (string.IsNullOrEmpty(request.Id))
                return Results.Json(ApiResponse<object>.OfCheckError("id is required"));

            var track = new ImageCaptchaTrack
            {
                BgImageWidth = request.Data.BgImageWidth,
                BgImageHeight = request.Data.BgImageHeight,
                TemplateImageWidth = request.Data.TemplateImageWidth,
                TemplateImageHeight = request.Data.TemplateImageHeight,
                StartTime = request.Data.StartTime,
                StopTime = request.Data.StopTime,
                Tracks = request.Data.Tracks ?? new()
            };

            var result = captchaApp.Matching(request.Id, track);
            return Results.Json(result);
        });

        group.MapPost(options.SecondaryVerifyEndpoint, (SecondaryVerifyRequest request, IImageCaptchaApplication captchaApp) =>
        {
            if (string.IsNullOrEmpty(request.Token))
                return Results.Json(ApiResponse<object>.OfCheckError("token is required"));

            var result = captchaApp.VerifySecondaryToken(request.Token);
            return Results.Json(result);
        });

        return app;
    }

    /// <summary>
    /// 映射验证码端点
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器</returns>
    public static IApplicationBuilder MapCaptchaEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints => {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<TianaiCaptchaOptions>>().Value;
            var group = endpoints.MapGroup(options.ApiEndpointPrefix);
            var captchaTypes = CaptchaTypeHelper.GetAll().ToArray();

            group.MapPost(options.GenerateEndpoint, (string type, IImageCaptchaApplication captchaApp) =>
            {
                CaptchaType captchaType;
    
                if (string.Equals(type, "RANDOM", StringComparison.OrdinalIgnoreCase))
                {
                    captchaType = captchaTypes[Random.Shared.Next(captchaTypes.Length)];
                }
                else if (string.IsNullOrEmpty(type))
                {
                    captchaType = CaptchaType.Slider;
                }
                else
                {
                    captchaType = Enum.Parse<CaptchaType>(type, true);
                }
    
                var result = captchaApp.GenerateCaptcha(captchaType);

                return Results.Json(result);
            });

            group.MapPost(options.ValidateEndpoint, (ValidateRequest request, IImageCaptchaApplication captchaApp) =>
            {
                if (string.IsNullOrEmpty(request.Id))
                    return Results.Json(ApiResponse<object>.OfCheckError("id is required"));

                var track = new ImageCaptchaTrack
                {
                    BgImageWidth = request.Data.BgImageWidth,
                    BgImageHeight = request.Data.BgImageHeight,
                    TemplateImageWidth = request.Data.TemplateImageWidth,
                    TemplateImageHeight = request.Data.TemplateImageHeight,
                    StartTime = request.Data.StartTime,
                    StopTime = request.Data.StopTime,
                    Tracks = request.Data.Tracks ?? new()
                };

                var result = captchaApp.Matching(request.Id, track);
                return Results.Json(result);
            });

            group.MapPost(options.SecondaryVerifyEndpoint, (SecondaryVerifyRequest request, IImageCaptchaApplication captchaApp) =>
            {
                if (string.IsNullOrEmpty(request.Token))
                    return Results.Json(ApiResponse<object>.OfCheckError("token is required"));

                var result = captchaApp.VerifySecondaryToken(request.Token);
                return Results.Json(result);
            });
        });
        return app;
    }

    private class ValidateRequest
    {
        public string? Id { get; set; }
        
        public ValidateData Data { get; set; }
    }
    
    private class ValidateData
    {
        public int BgImageWidth { get; set; }
        public int BgImageHeight { get; set; }
        public int TemplateImageWidth { get; set; }
        public int TemplateImageHeight { get; set; }
        public long StartTime { get; set; }
        public long StopTime { get; set; }
        public List<ImageCaptchaTrack.Track>? Tracks { get; set; }
    }

    private class SecondaryVerifyRequest
    {
        public string? Token { get; set; }
    }
}
