using Microsoft.AspNetCore.Mvc;
using Tianai.Captcha.Core.Application;
using Tianai.Captcha.Core.Common;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Sample.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IImageCaptchaApplication _captchaApp;

    public ResourcesController(IImageCaptchaApplication captchaApp)
    {
        _captchaApp = captchaApp;
    }

    [HttpGet("list")]
    public IActionResult ListResources()
    {
        var resourceManager = _captchaApp.GetImageCaptchaResourceManager();
        var store = resourceManager.GetResourceStore() as ICrudResourceStore;
        
        if (store == null)
        {
            return Ok(new { message = "Resource store is not a ICrudResourceStore" });
        }

        var resources = new Dictionary<string, object>();
        
        // List resources for each captcha type
        foreach (var captchaType in CaptchaTypeHelper.GetAll())
        {
            var typeName = captchaType.ToString();
            var typeResources = store.ListResourcesByTypeAndTag(captchaType, null);
            resources[typeName] = typeResources.Select(r => new { r.Id, r.Data, r.Type });
        }
        
        // List templates for each captcha type
        foreach (var captchaType in CaptchaTypeHelper.GetAll())
        {
            var typeName = captchaType.ToString();
            var typeTemplates = store.ListTemplatesByTypeAndTag(captchaType, null);
            resources[$"{typeName}Templates"] = typeTemplates.Select(t => new {
                t.Id,
                t.Tag,
                resources = t.Resources.Select(r => new { key = r.Key, data = r.Value.Data })
            });
        }

        return Ok(resources);
    }
}