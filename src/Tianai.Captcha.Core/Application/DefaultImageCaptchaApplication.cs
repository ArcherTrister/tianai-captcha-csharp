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

    protected DefaultImageCaptchaApplication()
    {
        
    }

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
        _logger.LogDebug("开始生成验证码: type={Type}", param.CaptchaType);
        // 优先从预生成池获取验证码
        if (_pregenerationPool != null)
        {
            var captchaType = param.CaptchaType;
            _logger.LogDebug("尝试从预生成池获取验证码: type={Type}", captchaType);
            var pregeneratedCaptcha = _pregenerationPool.GetCaptcha(captchaType);
            if (pregeneratedCaptcha != null && !pregeneratedCaptcha.IsExpired())
            {
                _logger.LogDebug("从预生成池成功获取验证码: id={Id}", pregeneratedCaptcha.Id);
                // 缓存验证数据
                if (pregeneratedCaptcha.ValidData.Count > 0)
                {
                    _logger.LogDebug("缓存预生成验证码验证数据: id={Id}", pregeneratedCaptcha.Id);
                    CacheVerification(pregeneratedCaptcha.Id, pregeneratedCaptcha.Type, pregeneratedCaptcha.ValidData);
                }
                return ApiResponse<ImageCaptchaResponse>.OfSuccess(pregeneratedCaptcha.Response);
            }
            _logger.LogDebug("预生成池无可用验证码，回退到实时生成");
        }

        // 预生成池为空或无可用验证码，回退到实时生成
        var context = _interceptor.CreateContext();
        var type = param.CaptchaType;

        // Before generate captcha
        _logger.LogDebug("执行验证码生成前拦截器");
        var beforeResult = _interceptor.BeforeGenerateCaptcha(context, type, param);
        if (beforeResult != null)
        {
            _logger.LogDebug("验证码生成前拦截器返回结果");
            return beforeResult;
        }

        // Generate ID: type + "_" + UUID (matches Java)
        var id = GeneratorId(param);
        _logger.LogDebug("生成验证码ID: {Id}", id);

        // Generate image
        _logger.LogDebug("开始生成验证码图片: type={Type}", type);
        var info = _generator.GenerateCaptchaImage(param);
        if (info == null)
        {
            _logger.LogError("生成验证码失败，验证码生成为空");
            throw new ImageCaptchaException("生成验证码失败，验证码生成为空");
        }
        _logger.LogDebug("验证码图片生成成功: type={Type}, id={Id}", type, id);

        // Before generate valid data
        _logger.LogDebug("执行验证码验证数据生成前拦截器");
        var beforeValidDataResult = _interceptor.BeforeGenerateImageCaptchaValidData(context, type, info);
        if (beforeValidDataResult != null)
        {
            _logger.LogDebug("验证码验证数据生成前拦截器返回结果");
            return beforeValidDataResult;
        }

        // Generate validation data
        _logger.LogDebug("开始生成验证码验证数据");
        var validData = _validator.GenerateImageCaptchaValidData(info);
        _logger.LogDebug("验证码验证数据生成完成: 数据项数={Count}", validData.Count);

        // After generate valid data
        _logger.LogDebug("执行验证码验证数据生成后拦截器");
        _interceptor.AfterGenerateImageCaptchaValidData(context, type, info, validData);

        // Cache validation data
        if (validData.Count > 0)
        {
            _logger.LogDebug("缓存验证码验证数据: id={Id}", id);
            CacheVerification(id, type, validData);
        }

        // Create response
        _logger.LogDebug("创建验证码响应: id={Id}", id);
        var response = ConvertToCaptchaResponse(id, info);

        // After generate captcha
        _logger.LogDebug("执行验证码生成后拦截器");
        _interceptor.AfterGenerateCaptcha(context, type, info, response);

        param.RemoveParam(ParamKeyEnum.Id);
        _logger.LogDebug("验证码生成完成: id={Id}, type={Type}", id, type);
        return response;
    }

    /// <summary>
    /// 生成验证码但不缓存，用于预生成池
    /// </summary>
    /// <param name="param">生成参数</param>
    /// <returns>预生成的验证码信息</returns>
    public (ApiResponse<ImageCaptchaResponse> Response, AnyMap ValidData) GenerateCaptchaWithoutCache(GenerateParam param)
    {
        _logger.LogDebug("开始生成预生成验证码: type={Type}", param.CaptchaType);
        var context = _interceptor.CreateContext();
        var type = param.CaptchaType;

        // Before generate captcha
        _logger.LogDebug("执行预生成验证码生成前拦截器");
        var beforeResult = _interceptor.BeforeGenerateCaptcha(context, type, param);
        if (beforeResult != null)
        {
            _logger.LogDebug("预生成验证码生成前拦截器返回结果");
            return (beforeResult, new AnyMap());
        }

        // Generate ID: type + "_" + UUID (matches Java)
        var id = GeneratorId(param);
        _logger.LogDebug("生成预生成验证码ID: {Id}", id);

        // Generate image
        _logger.LogDebug("开始生成预生成验证码图片: type={Type}", type);
        var info = _generator.GenerateCaptchaImage(param);
        if (info == null)
        {
            _logger.LogError("生成预生成验证码失败，验证码生成为空");
            throw new ImageCaptchaException("生成验证码失败，验证码生成为空");
        }
        _logger.LogDebug("预生成验证码图片生成成功: type={Type}, id={Id}", type, id);

        // Before generate valid data
        _logger.LogDebug("执行预生成验证码验证数据生成前拦截器");
        var beforeValidDataResult = _interceptor.BeforeGenerateImageCaptchaValidData(context, type, info);
        if (beforeValidDataResult != null)
        {
            _logger.LogDebug("预生成验证码验证数据生成前拦截器返回结果");
            return (beforeValidDataResult, new AnyMap());
        }

        // Generate validation data
        _logger.LogDebug("开始生成预生成验证码验证数据");
        var validData = _validator.GenerateImageCaptchaValidData(info);
        _logger.LogDebug("预生成验证码验证数据生成完成: 数据项数={Count}", validData.Count);

        // After generate valid data
        _logger.LogDebug("执行预生成验证码验证数据生成后拦截器");
        _interceptor.AfterGenerateImageCaptchaValidData(context, type, info, validData);

        // Create response
        _logger.LogDebug("创建预生成验证码响应: id={Id}", id);
        var response = ConvertToCaptchaResponse(id, info);

        // After generate captcha
        _logger.LogDebug("执行预生成验证码生成后拦截器");
        _interceptor.AfterGenerateCaptcha(context, type, info, response);

        param.RemoveParam(ParamKeyEnum.Id);
        _logger.LogDebug("预生成验证码生成完成: id={Id}, type={Type}", id, type);
        return (response, validData);
    }

    public ApiResponse<object> Matching(string id, ImageCaptchaTrack track)
    {
        return Matching(id, MatchParam.Of(track));
    }

    public ApiResponse<object> Matching(string id, MatchParam matchParam)
    {
        _logger.LogDebug("开始验证验证码: id={Id}", id);
        var validData = GetVerification(id);
        if (validData == null)
        {
            _logger.LogDebug("验证码验证数据已过期或不存在: id={Id}", id);
            return ApiResponse<object>.OfMessage(ApiResponseStatusConstant.Expired);
        }

        // todo: Enum.Parse<CaptchaType>(typeStr)
        var typeStr = GetCaptchaTypeById(id);
        var type = typeStr != null ? Enum.Parse<CaptchaType>(typeStr) : CaptchaType.Slider;
        _logger.LogDebug("解析验证码类型: id={Id}, type={Type}", id, type);
        var context = _interceptor.CreateContext();

        // todo: type.ToString()
        // Before valid
        _logger.LogDebug("执行验证码验证前拦截器: id={Id}", id);
        var beforeResult = _interceptor.BeforeValid(context, type.ToString(), matchParam, validData);
        if (beforeResult != null && !beforeResult.IsSuccess())
        {
            _logger.LogDebug("验证码验证前拦截器返回失败结果: id={Id}", id);
            return beforeResult;
        }

        // Validate
        var track = matchParam.Track ?? throw new ImageCaptchaException("Track is required");
        _logger.LogDebug("开始验证验证码轨迹: id={Id}", id);
        var basicValid = _validator.Valid(track, validData);
        _logger.LogDebug("验证码轨迹验证完成: id={Id}, success={Success}", id, basicValid.IsSuccess());

        // todo: type.ToString()
        // After valid
        _logger.LogDebug("执行验证码验证后拦截器: id={Id}", id);
        var afterResult = _interceptor.AfterValid(context, type.ToString(), matchParam, validData, basicValid);
        var result = afterResult ?? basicValid;
        _logger.LogDebug("验证码验证最终结果: id={Id}, success={Success}", id, result.IsSuccess());

        // 二次验证处理
        if (result.IsSuccess())
        {
            var secondaryToken = GenerateSecondaryToken();
            _logger.LogDebug("生成二次验证令牌: token={Token}", secondaryToken);
            CacheSecondaryVerification(secondaryToken);
            result.Data = new { success = true, secondaryToken };
            _logger.LogDebug("验证码验证成功，已生成二次验证令牌: id={Id}", id);
        }

        _logger.LogDebug("验证码验证完成: id={Id}, success={Success}", id, result.IsSuccess());
        return result;
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

        var tolerant = cachePercentage.GetFloat(SimpleImageCaptchaValidator.TolerantKey, _options.DefaultTolerant);
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

    /// <summary>
    /// 生成二次验证令牌
    /// </summary>
    protected string GenerateSecondaryToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 缓存二次验证数据
    /// </summary>
    protected void CacheSecondaryVerification(string token)
    {
        var key = GetSecondaryKey(token);
        var expire = _options.SecondaryVerifyExpire;
        var data = new AnyMap { { "valid", true } };
        
        if (!_cacheStore.SetCache(key, data, expire, TimeUnit.Milliseconds))
        {
            _logger.LogError("缓存二次验证数据失败, token={Token}", token);
        }
    }

    /// <summary>
    /// 获取二次验证键
    /// </summary>
    protected string GetSecondaryKey(string token)
    {
        return _options.SecondaryVerifyKeyPrefix + ":" + token;
    }

    /// <summary>
    /// 验证二次验证令牌
    /// </summary>
    public ApiResponse<object> VerifySecondaryToken(string token)
    {
        _logger.LogDebug("开始验证二次验证令牌: token={Token}", token);
        var key = GetSecondaryKey(token);
        _logger.LogDebug("生成二次验证令牌缓存键: key={Key}", key);
        var validData = _cacheStore.GetAndRemoveCache(key);
        
        if (validData == null)
        {
            _logger.LogDebug("二次验证令牌已过期或不存在: token={Token}", token);
            return ApiResponse<object>.OfMessage(ApiResponseStatusConstant.Expired);
        }

        _logger.LogDebug("二次验证令牌验证成功: token={Token}", token);
        return ApiResponse<object>.OfSuccess(new { success = true });
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
