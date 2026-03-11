namespace Tianai.Captcha.Core.Common;

public class ImageCaptchaTrack
{
    public int BgImageWidth { get; set; }
    public int BgImageHeight { get; set; }
    public int TemplateImageWidth { get; set; }
    public int TemplateImageHeight { get; set; }
    public long StartTime { get; set; }
    public long StopTime { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public List<Track> Tracks { get; set; } = new();
    public object? Data { get; set; }

    public class Track
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float T { get; set; }
        public string Type { get; set; } = TrackTypes.Move;

        public Track() { }

        public Track(float x, float y, float t, string type = TrackTypes.Move)
        {
            X = x; Y = y; T = t; Type = type;
        }
    }
}
