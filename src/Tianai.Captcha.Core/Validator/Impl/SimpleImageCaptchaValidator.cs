using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Validator.Impl;

public class SimpleImageCaptchaValidator : IImageCaptchaValidator
{
    public const string PercentageKey = "percentage";
    public const string TolerantKey = "tolerant";
    public const string TypeKey = "type";
    public const string ClickImageCheckOrderKey = "click_image_check_order";

    private readonly ILogger _logger;
    private readonly ImageCaptchaOptions _options;

    public SimpleImageCaptchaValidator(ImageCaptchaOptions options, ILogger<SimpleImageCaptchaValidator>? logger = null)
    {
        _logger = logger ?? NullLogger<SimpleImageCaptchaValidator>.Instance;
        _options = options;
    }

    // 为了支持依赖注入，添加一个接受IOptions<ImageCaptchaOptions>的构造函数
    public SimpleImageCaptchaValidator(IOptions<ImageCaptchaOptions> options, ILogger<SimpleImageCaptchaValidator>? logger = null) : this(options.Value, logger)
    {
    }

    // 为了保持测试的兼容性，添加一个无参数构造函数
    public SimpleImageCaptchaValidator() : this(new ImageCaptchaOptions())
    {
    }

    public AnyMap GenerateImageCaptchaValidData(ImageCaptchaInfo info)
    {
        var map = AnyMap.Create();
        var typeStr = info.Type.ToString().ToUpper();
        map[TypeKey] = typeStr;

        if (CaptchaTypeClassifier.IsSliderCaptcha(typeStr))
        {
            AddPercentage(info, map);
        }
        else if (CaptchaTypeClassifier.IsClickCaptcha(typeStr))
        {
            AddClickCheckData(info, map);
        }

        return map;
    }

    private void AddPercentage(ImageCaptchaInfo info, AnyMap map)
    {
        if (info.RandomX == null || info.BackgroundImageWidth <= 0) return;

        float percentage = (float)info.RandomX.Value / info.BackgroundImageWidth;
        map[PercentageKey] = percentage;

        float tolerant = info.Tolerant.HasValue && info.Tolerant.Value > 0 ? info.Tolerant.Value : _options.SliderTolerant;
        map[TolerantKey] = tolerant;
    }

    private void AddClickCheckData(ImageCaptchaInfo info, AnyMap map)
        {
            if (info.Data?.Data == null) return;

            if (info.Data.Data.TryGetValue("checkDefinitions", out var defs) && defs is List<ClickImageCheckDefinition> checkDefs)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < checkDefs.Count; i++)
                {
                    var def = checkDefs[i];
                    // Java stores center coordinates (block.startX + clickImgWidth/2);
                    // .NET generator stores left-top, so compute center here
                    float centerX = def.X + def.Width / 2f;
                    float centerY = def.Y + def.Height / 2f;
                    float vx = CalcPercentage(centerX, info.BackgroundImageWidth);
                    float vy = CalcPercentage(centerY, info.BackgroundImageHeight);
                    sb.Append($"{vx.ToString(CultureInfo.InvariantCulture)},{vy.ToString(CultureInfo.InvariantCulture)};");

                    // Java: dynamically calculate tolerant from first character's width
                    // tolerant = (charWidth / 2) / bgWidth
                    if (i == 0 && !map.ContainsKey(TolerantKey))
                    {
                        float tolerant = info.Tolerant.HasValue && info.Tolerant.Value > 0 ? info.Tolerant.Value : CalcPercentage(def.Width / 2f, info.BackgroundImageWidth);
                        map[TolerantKey] = tolerant;
                    }
                }
                map[PercentageKey] = sb.ToString();
                map[ClickImageCheckOrderKey] = true;
            }
        }

    public ApiResponse<object> Valid(ImageCaptchaTrack track, AnyMap validData)
    {
        // todo: type.ToString()
        var typeStr = validData.GetString(TypeKey, CaptchaType.Slider.ToString())!;
        var type = Enum.Parse<CaptchaType>(typeStr, true); // Use ignoreCase=true
        float defaultTolerant;
        switch (type)
        {
            case CaptchaType.Slider:
                defaultTolerant = _options.SliderTolerant;
                break;
            case CaptchaType.Rotate:
                defaultTolerant = _options.RotateTolerant;
                break;
            case CaptchaType.Concat:
                defaultTolerant = _options.ConcatTolerant;
                break;
            case CaptchaType.WordImageClick:
                defaultTolerant = _options.WordImageClickTolerant;
                break;
            default:
                defaultTolerant = _options.DefaultTolerant;
                break;
        }
        var tolerant = validData.GetFloat(TolerantKey, defaultTolerant)!.Value;

        bool valid;
        if (CaptchaTypeClassifier.IsSliderCaptcha(type))
        {
            valid = DoValidSliderCaptcha(track, validData, tolerant);
        }
        else if (CaptchaTypeClassifier.IsClickCaptcha(type))
        {
            valid = DoValidClickCaptcha(track, validData, tolerant);
        }
        else
        {
            valid = false;
        }

        return valid ? ApiResponse<object>.OfSuccess() : ApiResponse<object>.OfMessage(ApiResponseStatusConstant.BasicCheckFail);
    }

    private bool DoValidSliderCaptcha(ImageCaptchaTrack imageCaptchaTrack, AnyMap validData, float tolerant)
    {
        var oriPercentage = validData.GetFloat(PercentageKey);
        if (oriPercentage == null) return false;

        if (imageCaptchaTrack.Tracks.Count == 0) return false;

        var firstTrack = imageCaptchaTrack.Tracks[0];
        var lastTrack = imageCaptchaTrack.Tracks[^1];
        float calcPercentage = CalcPercentage(lastTrack.X - firstTrack.X, imageCaptchaTrack.BgImageWidth);

        return CheckPercentage(calcPercentage, oriPercentage.Value, tolerant);
    }

    private bool DoValidClickCaptcha(ImageCaptchaTrack imageCaptchaTrack, AnyMap validData, float tolerant)
    {
        var validStr = validData.GetString(PercentageKey);
        if (string.IsNullOrEmpty(validStr)) return false;

        var splitArr = validStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var clickTracks = imageCaptchaTrack.Tracks.Where(t => t.Type == TrackTypes.Click).ToList();

        if (clickTracks.Count != splitArr.Length) return false;

        for (int i = 0; i < splitArr.Length; i++)
        {
            var parts = splitArr[i].Split(',');
            if (parts.Length < 2) return false;

            float xPercentage = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float yPercentage = float.Parse(parts[1], CultureInfo.InvariantCulture);

            var clickTrack = clickTracks[i];
            float calcXPercentage = CalcPercentage(clickTrack.X, imageCaptchaTrack.BgImageWidth);
            float calcYPercentage = CalcPercentage(clickTrack.Y, imageCaptchaTrack.BgImageHeight);

            if (!CheckPercentage(calcXPercentage, xPercentage, tolerant)
                || !CheckPercentage(calcYPercentage, yPercentage, tolerant))
                return false;
        }

        return true;
    }

    public static float CalcPercentage(float pos, float maxPos)
    {
        if (maxPos == 0) return 0;
        return pos / maxPos;
    }

    public static bool CheckPercentage(float newPercentage, float oriPercentage, float tolerant)
    {
        return newPercentage >= oriPercentage - tolerant && newPercentage <= oriPercentage + tolerant;
    }
}
