using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Validator.Impl;

namespace Tianai.Captcha.Core.Tests.Validator;

public class SimpleImageCaptchaValidatorTests
{
    private readonly SimpleImageCaptchaValidator _validator = new();

    [Fact]
    public void CalcPercentage_NormalValues_CalculatesCorrectly()
    {
        Assert.Equal(0.5f, SimpleImageCaptchaValidator.CalcPercentage(50, 100));
        Assert.Equal(0.25f, SimpleImageCaptchaValidator.CalcPercentage(25, 100));
    }

    [Fact]
    public void CalcPercentage_ZeroMax_ReturnsZero()
    {
        Assert.Equal(0, SimpleImageCaptchaValidator.CalcPercentage(50, 0));
    }

    [Fact]
    public void CheckPercentage_WithinTolerant_ReturnsTrue()
    {
        Assert.True(SimpleImageCaptchaValidator.CheckPercentage(0.50f, 0.51f, 0.02f));
        Assert.True(SimpleImageCaptchaValidator.CheckPercentage(0.50f, 0.50f, 0.02f));
    }

    [Fact]
    public void CheckPercentage_OutsideTolerant_ReturnsFalse()
    {
        Assert.False(SimpleImageCaptchaValidator.CheckPercentage(0.40f, 0.50f, 0.02f));
        Assert.False(SimpleImageCaptchaValidator.CheckPercentage(0.60f, 0.50f, 0.02f));
    }

    [Fact]
    public void GenerateImageCaptchaValidData_SliderType_ContainsPercentage()
    {
        var info = SliderImageCaptchaInfo.Of(
            x: 150, y: 50,
            backgroundImage: "bg", templateImage: "tpl",
            backgroundImageTag: "tag1", templateImageTag: "tag2",
            backgroundImageWidth: 300, backgroundImageHeight: 200,
            templateImageWidth: 50, templateImageHeight: 50,
            type: CaptchaType.Slider);

        var validData = _validator.GenerateImageCaptchaValidData(info);

        Assert.Equal("SLIDER", validData.GetString(SimpleImageCaptchaValidator.TypeKey));
        var percentage = validData.GetFloat(SimpleImageCaptchaValidator.PercentageKey);
        Assert.NotNull(percentage);
        Assert.Equal(0.5f, percentage.Value, 0.001f);
    }

    [Fact]
    public void Valid_SliderCorrectTrack_ReturnsSuccess()
    {
        // Generate valid data for a slider at x=150 out of width=300 => percentage = 0.5
        var info = SliderImageCaptchaInfo.Of(
            x: 150, y: 50,
            backgroundImage: "bg", templateImage: "tpl",
            backgroundImageTag: null, templateImageTag: null,
            backgroundImageWidth: 300, backgroundImageHeight: 200,
            templateImageWidth: 50, templateImageHeight: 50,
            type: CaptchaType.Slider);

        var validData = _validator.GenerateImageCaptchaValidData(info);

        // User track: moved from x=0 to x=150 (percentage = 150/300 = 0.5)
        var track = new ImageCaptchaTrack
        {
            BgImageWidth = 300,
            BgImageHeight = 200,
            Tracks = new List<ImageCaptchaTrack.Track>
            {
                new(0, 100, 0),
                new(50, 100, 100),
                new(150, 100, 300)
            }
        };

        var result = _validator.Valid(track, validData);
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public void Valid_SliderWrongTrack_ReturnsFail()
    {
        var info = SliderImageCaptchaInfo.Of(
            x: 150, y: 50,
            backgroundImage: "bg", templateImage: "tpl",
            backgroundImageTag: null, templateImageTag: null,
            backgroundImageWidth: 300, backgroundImageHeight: 200,
            templateImageWidth: 50, templateImageHeight: 50,
            type: CaptchaType.Slider);

        var validData = _validator.GenerateImageCaptchaValidData(info);

        // User track: moved from x=0 to x=50 (too far off)
        var track = new ImageCaptchaTrack
        {
            BgImageWidth = 300,
            BgImageHeight = 200,
            Tracks = new List<ImageCaptchaTrack.Track>
            {
                new(0, 100, 0),
                new(50, 100, 300)
            }
        };

        var result = _validator.Valid(track, validData);
        Assert.False(result.IsSuccess());
        Assert.Equal(4001, result.Code);
    }

    [Fact]
    public void Valid_EmptyTrackList_ReturnsFail()
    {
        var validData = new AnyMap
        {
            [SimpleImageCaptchaValidator.TypeKey] = "SLIDER",
            [SimpleImageCaptchaValidator.PercentageKey] = 0.5f,
            [SimpleImageCaptchaValidator.TolerantKey] = 0.02f
        };

        var track = new ImageCaptchaTrack
        {
            BgImageWidth = 300,
            BgImageHeight = 200,
            Tracks = new List<ImageCaptchaTrack.Track>()
        };

        var result = _validator.Valid(track, validData);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public void GenerateImageCaptchaValidData_RotateType_ContainsPercentage()
    {
        var info = RotateImageCaptchaInfo.Of(
            degree: 90,
            backgroundImage: "bg", templateImage: "tpl",
            backgroundImageTag: null, templateImageTag: null,
            backgroundImageWidth: 300, backgroundImageHeight: 300,
            templateImageWidth: 200, templateImageHeight: 200,
            randomX: 75, type: CaptchaType.Rotate);

        var validData = _validator.GenerateImageCaptchaValidData(info);
        Assert.Equal("ROTATE", validData.GetString(SimpleImageCaptchaValidator.TypeKey));
        Assert.NotNull(validData.GetFloat(SimpleImageCaptchaValidator.PercentageKey));
    }

    [Fact]
    public void Valid_ClickCorrectPositions_ReturnsSuccess()
    {
        // Simulate 4 characters placed on a 600x360 background
        // Java stores top-left coordinates, tolerance = charWidth/2 / bgWidth
        var checkDefs = new List<ClickImageCheckDefinition>
        {
            new() { X = 100, Y = 50, Width = 50, Height = 50 },
            new() { X = 300, Y = 200, Width = 50, Height = 50 },
            new() { X = 450, Y = 100, Width = 50, Height = 50 },
            new() { X = 50, Y = 280, Width = 50, Height = 50 },
        };

        var info = new ImageCaptchaInfo
        {
            BackgroundImageWidth = 600,
            BackgroundImageHeight = 360,
            Type = CaptchaType.WordImageClick,
            Data = new CustomData()
        };
        info.Data.Data["checkDefinitions"] = checkDefs;
        info.Data.ViewData["wordList"] = new List<string> { "A", "B", "C", "D" };

        var validData = _validator.GenerateImageCaptchaValidData(info);
        Assert.NotNull(validData.GetString(SimpleImageCaptchaValidator.PercentageKey));

        // User clicks near center of each character (display at 310x186)
        // tolerant = charWidth/2/bgWidth = 25/600 ≈ 0.0417
        float displayW = 310f;
        float displayH = 186f;
        var track = new ImageCaptchaTrack
        {
            BgImageWidth = (int)displayW,
            BgImageHeight = (int)displayH,
            Tracks = checkDefs.Select((d, i) => new ImageCaptchaTrack.Track(
                (d.X + d.Width / 2f) / 600f * displayW + 3,  // slightly off from center
                (d.Y + d.Height / 2f) / 360f * displayH + 3,
                i * 500f,
                TrackTypes.Click
            )).ToList()
        };

        var result = _validator.Valid(track, validData);
        Assert.True(result.IsSuccess(), "Click validation should succeed with positions near center");
    }

    [Fact]
    public void Valid_ClickWrongPositions_ReturnsFail()
    {
        var checkDefs = new List<ClickImageCheckDefinition>
        {
            new() { X = 100, Y = 50, Width = 50, Height = 50 },
            new() { X = 300, Y = 200, Width = 50, Height = 50 },
            new() { X = 450, Y = 100, Width = 50, Height = 50 },
            new() { X = 50, Y = 280, Width = 50, Height = 50 },
        };

        var info = new ImageCaptchaInfo
        {
            BackgroundImageWidth = 600,
            BackgroundImageHeight = 360,
            Type = CaptchaType.WordImageClick,
            Data = new CustomData()
        };
        info.Data.Data["checkDefinitions"] = checkDefs;
        info.Data.ViewData["wordList"] = new List<string> { "A", "B", "C", "D" };

        var validData = _validator.GenerateImageCaptchaValidData(info);

        // User clicks at completely wrong positions
        var track = new ImageCaptchaTrack
        {
            BgImageWidth = 310,
            BgImageHeight = 186,
            Tracks = new List<ImageCaptchaTrack.Track>
            {
                new(10, 10, 0, TrackTypes.Click),
                new(20, 20, 500, TrackTypes.Click),
                new(30, 30, 1000, TrackTypes.Click),
                new(40, 40, 1500, TrackTypes.Click),
            }
        };

        var result = _validator.Valid(track, validData);
        Assert.False(result.IsSuccess(), "Click validation should fail with wrong positions");
    }

    [Fact]
    public void Valid_ClickWrongCount_ReturnsFail()
    {
        var checkDefs = new List<ClickImageCheckDefinition>
        {
            new() { X = 100, Y = 50, Width = 50, Height = 50 },
            new() { X = 300, Y = 200, Width = 50, Height = 50 },
        };

        var info = new ImageCaptchaInfo
        {
            BackgroundImageWidth = 600,
            BackgroundImageHeight = 360,
            Type = CaptchaType.WordImageClick,
            Data = new CustomData()
        };
        info.Data.Data["checkDefinitions"] = checkDefs;
        info.Data.ViewData["wordList"] = new List<string> { "A", "B" };

        var validData = _validator.GenerateImageCaptchaValidData(info);

        // User only clicks once (expected 2)
        var track = new ImageCaptchaTrack
        {
            BgImageWidth = 310,
            BgImageHeight = 186,
            Tracks = new List<ImageCaptchaTrack.Track>
            {
                new(50, 50, 0, TrackTypes.Click),
            }
        };

        var result = _validator.Valid(track, validData);
        Assert.False(result.IsSuccess(), "Click validation should fail with wrong click count");
    }
}
