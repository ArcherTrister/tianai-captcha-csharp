using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource;

public class ResourceMap
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public Dictionary<string, CaptchaResource> Resources { get; set; } = new();
    public string? Tag { get; set; }

    public CaptchaResource? Get(string key) => Resources.GetValueOrDefault(key);
    public void Put(string key, CaptchaResource resource) => Resources[key] = resource;
    public CaptchaResource? Remove(string key) { Resources.Remove(key, out var v); return v; }
    public ICollection<CaptchaResource> Values => Resources.Values;
}
