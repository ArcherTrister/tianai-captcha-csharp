using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource;

public interface IResourceProvider
{
    string Name { get; }
    bool Supported(CaptchaResource resource);
    Stream GetResourceStream(CaptchaResource resource);
}
