namespace Tianai.Captcha.Core.Common;

public class MatchParam : AnyMap
{
    private static readonly ParamKey<ImageCaptchaTrack> TrackKey = new("track");

    public ImageCaptchaTrack? Track
    {
        get => GetParam(TrackKey);
        set => AddParam(TrackKey, value!);
    }

    public static MatchParam Of(ImageCaptchaTrack track)
    {
        var param = new MatchParam { Track = track };
        return param;
    }
}
