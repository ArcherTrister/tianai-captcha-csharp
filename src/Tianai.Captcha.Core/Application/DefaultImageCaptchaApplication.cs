using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Cache;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Generator;
using Tianai.Captcha.Core.Interceptor;
using Tianai.Captcha.Core.Pregeneration;
using Tianai.Captcha.Core.Resource;
using Tianai.Captcha.Core.Validator;
using Tianai.Captcha.Core.Validator.Impl;

namespace Tianai.Captcha.Core.Application;

public class DefaultImageCaptchaApplication : IImageCaptchaApplication
{
    public const string IdSplit = "_";

    private readonly IImageCaptchaGenerator _generator;
    private readonly IImageCaptchaValidator _validator;
    private readonly ICacheStore _cacheStore;
    private readonly ImageCaptchaOptions _options;
    private readonly ICaptchaInterceptor _interceptor;
    private readonly ILogger _logger;
    private readonly ICaptchaPregenerationPool? _pregenerationPool;

    public DefaultImageCaptchaApplication(
        IImageCaptchaGenerator generator,
        IImageCaptchaValidator validator,
        ICacheStore cacheStore,
        ImageCaptchaOptions options,
        ICaptchaInterceptor interceptor,
        ICaptchaPregenerationPool? pregenerationPool = null,
        ILogger<DefaultImageCaptchaApplication>? logger = null)
    {
        _generator = generator;
        _validator = validator;
        _cacheStore = cacheStore;
        _options = options;
        _interceptor = interceptor;
        _pregenerationPool = pregenerationPool;
        _logger = logger ?? NullLogger<DefaultImageCaptchaApplication>.Instance;
    }

    public ApiResponse<ImageCaptchaResponse> GenerateCaptcha()
    {
        return GenerateCaptcha(CaptchaType.Slider);
    }

    public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type)
    {
        var param = new GenerateParam { CaptchaType = type };
        return GenerateCaptcha(param);
    }

    public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaImageType captchaImageType)
    {
        return GenerateCaptcha(CaptchaType.Slider, captchaImageType);
    }

    public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(CaptchaType type, CaptchaImageType captchaImageType)
    {
        var param = new GenerateParam { CaptchaType = type };
        if (captchaImageType == CaptchaImageType.WebP)
        {
            param.BackgroundFormatName = "webp";
            param.TemplateFormatName = "webp";
        }
        else
        {
            param.BackgroundFormatName = "jpeg";
            param.TemplateFormatName = "png";
        }
        return GenerateCaptcha(param);
    }

    public ApiResponse<ImageCaptchaResponse> GenerateCaptcha(GenerateParam param)
    {
        // 优先从预生成池获取验证码
        if (_pregenerationPool != null)
        {
            var pregeneratedCaptcha = _pregenerationPool.GetCaptcha();
            if (pregeneratedCaptcha != null && !pregeneratedCaptcha.IsExpired())
            {
                // 缓存验证数据
                if (pregeneratedCaptcha.ValidData.Count > 0)
                {
                    CacheVerification(pregeneratedCaptcha.Id, pregeneratedCaptcha.Type, pregeneratedCaptcha.ValidData);
                }
                return ApiResponse<ImageCaptchaResponse>.OfSuccess(pregeneratedCaptcha.Response);
            }
        }

        // 预生成池为空或无可用验证码，回退到实时生成
        var context = _interceptor.CreateContext();
        var type = param.CaptchaType;

        // Before generate captcha
        var beforeResult = _interceptor.BeforeGenerateCaptcha(context, type, param);
        if (beforeResult != null) return beforeResult;

        // Generate ID: type + "_" + UUID (matches Java)
        var id = GeneratorId(param);

        // Generate image
        var info = _generator.GenerateCaptchaImage(param);
        if (info == null)
            throw new ImageCaptchaException("生成验证码失败，验证码生成为空");

        // Before generate valid data
        var beforeValidDataResult = _interceptor.BeforeGenerateImageCaptchaValidData(context, type, info);
        if (beforeValidDataResult != null) return beforeValidDataResult;

        // Generate validation data
        var validData = _validator.GenerateImageCaptchaValidData(info);

        // After generate valid data
        _interceptor.AfterGenerateImageCaptchaValidData(context, type, info, validData);

        // Cache validation data
        if (validData.Count > 0)
        {
            CacheVerification(id, type, validData);
        }

        // Create response
        var response = ConvertToCaptchaResponse(id, info);

        // After generate captcha
        _interceptor.AfterGenerateCaptcha(context, type, info, response);

        param.RemoveParam(ParamKeyEnum.Id);
        return response;
    }

    /// <summary>
    /// 生成验证码但不缓存，用于预生成池
    /// </summary>
    /// <param name="param">生成参数</param>
    /// <returns>预生成的验证码信息</returns>
    public (ApiResponse<ImageCaptchaResponse> Response, AnyMap ValidData) GenerateCaptchaWithoutCache(GenerateParam param)
    {
        var context = _interceptor.CreateContext();
        var type = param.CaptchaType;

        // Before generate captcha
        var beforeResult = _interceptor.BeforeGenerateCaptcha(context, type, param);
        if (beforeResult != null) return (beforeResult, new AnyMap());

        // Generate ID: type + "_" + UUID (matches Java)
        var id = GeneratorId(param);

        // Generate image
        var info = _generator.GenerateCaptchaImage(param);
        if (info == null)
            throw new ImageCaptchaException("生成验证码失败，验证码生成为空");

        // Before generate valid data
        var beforeValidDataResult = _interceptor.BeforeGenerateImageCaptchaValidData(context, type, info);
        if (beforeValidDataResult != null) return (beforeValidDataResult, new AnyMap());

        // Generate validation data
        var validData = _validator.GenerateImageCaptchaValidData(info);

        // After generate valid data
        _interceptor.AfterGenerateImageCaptchaValidData(context, type, info, validData);

        // Create response
        var response = ConvertToCaptchaResponse(id, info);

        // After generate captcha
        _interceptor.AfterGenerateCaptcha(context, type, info, response);

        param.RemoveParam(ParamKeyEnum.Id);
        return (response, validData);
    }

    public ApiResponse<object> Matching(string id, ImageCaptchaTrack track)
    {
        return Matching(id, MatchParam.Of(track));
    }

    public ApiResponse<object> Matching(string id, MatchParam matchParam)
    {
        var validData = GetVerification(id);
        if (validData == null)
            return ApiResponse<object>.OfMessage(ApiResponseStatusConstant.Expired);

        var type = GetCaptchaTypeById(id) ?? CaptchaType.Slider.ToString();
        var context = _interceptor.CreateContext();

        // Before valid
        var beforeResult = _interceptor.BeforeValid(context, type, matchParam, validData);
        if (beforeResult != null && !beforeResult.IsSuccess()) return beforeResult;

        // Validate
        var track = matchParam.Track ?? throw new ImageCaptchaException("Track is required");
        var basicValid = _validator.Valid(track, validData);

        // After valid
        var afterResult = _interceptor.AfterValid(context, type, matchParam, validData, basicValid);
        return afterResult ?? basicValid;
    }

    /// <summary>
    /// 简单百分比匹配 (兼容 Java matching(String, Float))
    /// </summary>
    public bool Matching(string id, float percentage)
    {
        var cachePercentage = GetVerification(id);
        if (cachePercentage == null) return false;
        if (_validator is not SimpleImageCaptchaValidator simpleValidator) return false;

        var oriPercentage = cachePercentage.GetFloat(SimpleImageCaptchaValidator.PercentageKey);
        if (oriPercentage == null) return false;

        var tolerant = cachePercentage.GetFloat(SimpleImageCaptchaValidator.TolerantKey, simpleValidator.DefaultTolerant);
        return SimpleImageCaptchaValidator.CheckPercentage(percentage, oriPercentage.Value, tolerant!.Value);
    }

    /// <summary>
    /// 从 ID 中解析验证码类型 (格式: TYPE_uuid)
    /// </summary>
    public string? GetCaptchaTypeById(string id)
    {
        var parts = id.Split(IdSplit, 2);
        return parts.Length >= 2 ? parts[0] : null;
    }

    /// <summary>
    /// 生成 ID: type + "_" + UUID (与 Java 保持一致)
    /// 支持通过 ParamKeyEnum.Id 预设 ID
    /// </summary>
    protected virtual string GeneratorId(GenerateParam param)
    {
        var id = param.GetParam(ParamKeyEnum.Id);
        if (string.IsNullOrEmpty(id))
        {
            id = param.Type + IdSplit + Guid.NewGuid().ToString("N");
            param.AddParam(ParamKeyEnum.Id, id);
        }
        return id;
    }

    private ApiResponse<ImageCaptchaResponse> ConvertToCaptchaResponse(string id, ImageCaptchaInfo info)
    {
        var vo = new ImageCaptchaResponse
        {
            Id = id,
            Type = info.Type.ToString().ToUpper(),
            BackgroundImage = info.BackgroundImage,
            TemplateImage = info.TemplateImage,
            BackgroundImageTag = info.BackgroundImageTag,
            TemplateImageTag = info.TemplateImageTag,
            BackgroundImageWidth = info.BackgroundImageWidth,
            BackgroundImageHeight = info.BackgroundImageHeight,
            TemplateImageWidth = info.TemplateImageWidth,
            TemplateImageHeight = info.TemplateImageHeight,
            Data = info.Data?.ViewData
        };
        return ApiResponse<ImageCaptchaResponse>.OfSuccess(vo);
    }

    private AnyMap? GetVerification(string id)
    {
        return _cacheStore.GetAndRemoveCache(GetKey(id));
    }

    private void CacheVerification(string id, CaptchaType type, AnyMap validData)
    {
        var expire = _options.GetExpire(type);
        if (!_cacheStore.SetCache(GetKey(id), validData, expire, TimeUnit.Milliseconds))
        {
            _logger.LogError("缓存验证码数据失败, id={Id}", id);
            throw new ImageCaptchaException("缓存验证码数据失败: " + type);
        }
    }

    protected string GetKey(string id)
    {
        return _options.Prefix + ":" + id;
    }

    public IImageCaptchaResourceManager GetImageCaptchaResourceManager() => _generator.GetImageResourceManager();
    public IImageCaptchaValidator GetImageCaptchaValidator() => _validator;
    public IImageCaptchaGenerator GetImageCaptchaGenerator() => _generator;
    public ICacheStore GetCacheStore() => _cacheStore;
    public ICaptchaInterceptor GetCaptchaInterceptor() => _interceptor;

    public void Dispose()
    {
        if (_generator is IDisposable dg) dg.Dispose();
        if (_cacheStore is IDisposable dc) dc.Dispose();
    }
}
