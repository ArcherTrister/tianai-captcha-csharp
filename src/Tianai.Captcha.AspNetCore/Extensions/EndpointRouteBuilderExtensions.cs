using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.AspNetCore.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCaptchaEndpoints(this IEndpointRouteBuilder endpoints, string prefix = "/api/captcha")
    {
        var group = endpoints.MapGroup(prefix);

        group.MapPost("/generate", async (HttpContext context, IImageCaptchaApplication app) =>
        {
            string? type = null;
            try
            {
                var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);
                if (body.TryGetProperty("type", out var typeProp))
                    type = typeProp.GetString();
            }
            catch { /* use default type */ }

            var result = type != null ? app.GenerateCaptcha(Enum.Parse<CaptchaType>(type, true)) : app.GenerateCaptcha();
            return Results.Json(result);
        });

        group.MapPost("/validate", async (HttpContext context, IImageCaptchaApplication app) =>
        {
            var body = await JsonSerializer.DeserializeAsync<ValidateRequest>(context.Request.Body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (body == null || string.IsNullOrEmpty(body.Id))
                return Results.Json(ApiResponse<object>.OfCheckError("id is required"));

            var track = new ImageCaptchaTrack
            {
                BgImageWidth = body.BgImageWidth,
                BgImageHeight = body.BgImageHeight,
                TemplateImageWidth = body.TemplateImageWidth,
                TemplateImageHeight = body.TemplateImageHeight,
                StartTime = body.StartTime,
                StopTime = body.StopTime,
                Tracks = body.Tracks ?? new()
            };

            var result = app.Matching(body.Id, track);
            return Results.Json(result);
        });

        return endpoints;
    }

    private class ValidateRequest
    {
        public string? Id { get; set; }
        public int BgImageWidth { get; set; }
        public int BgImageHeight { get; set; }
        public int TemplateImageWidth { get; set; }
        public int TemplateImageHeight { get; set; }
        public long StartTime { get; set; }
        public long StopTime { get; set; }
        public List<ImageCaptchaTrack.Track>? Tracks { get; set; }
    }
}
