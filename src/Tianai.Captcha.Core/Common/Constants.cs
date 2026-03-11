namespace Tianai.Captcha.Core.Common;

public enum CaptchaType
{
    Slider = 0,
    Rotate = 1,
    Concat = 2,
    WordImageClick = 3
}

public static class CaptchaTypeHelper
{
    public static IEnumerable<CaptchaType> GetAll()
    {
        return Enum.GetValues(typeof(CaptchaType)).Cast<CaptchaType>();
    }
}

public static class TrackTypes
{
    public const string Move = "move";
    public const string Click = "click";
}

public static class CommonConstant
{
    public const string DefaultTag = "default";
    public const string ImageTipIcon = "tip";
    public const string ImageClickIcon = "click";
}
