using System.IO;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

/// <summary>
/// 资源验证器，用于验证资源的有效性
/// </summary>
public class ResourceValidator
{
    private readonly ILogger<ResourceValidator> _logger;

    public ResourceValidator(ILogger<ResourceValidator>? logger = null)
    {
        _logger = logger ?? NullLogger<ResourceValidator>.Instance;
    }

    /// <summary>
    /// 验证图片资源
    /// </summary>
    public bool ValidateImage(Stream stream)
    {
        try
        {
            // 保存当前位置
            var position = stream.Position;
            
            // 简单验证：检查流长度是否大于 0
            if (stream.Length > 0)
            {
                // 重置流位置
                stream.Position = position;
                _logger.LogDebug("图片资源验证成功: 长度={Length}", stream.Length);
                return true;
            }
            
            // 重置流位置
            stream.Position = position;
            _logger.LogDebug("图片资源验证失败: 长度为 0");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("图片资源验证异常: {ExceptionMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 验证字体资源
    /// </summary>
    public bool ValidateFont(Stream stream)
    {
        try
        {
            // 简单验证字体文件大小
            var isValid = stream.Length > 0;
            if (isValid)
            {
                _logger.LogDebug("字体资源验证成功: 长度={Length}", stream.Length);
            }
            else
            {
                _logger.LogDebug("字体资源验证失败: 长度为 0");
            }
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("字体资源验证异常: {ExceptionMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 根据文件扩展名验证资源类型
    /// </summary>
    public bool ValidateByExtension(string fileName, string resourceType)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        
        switch (resourceType)
        {
            case "BgImages":
            case "Templates":
                var isImage = IsImageExtension(extension);
                _logger.LogDebug("验证图片扩展名: fileName={FileName}, extension={Extension}, isValid={IsValid}", fileName, extension, isImage);
                return isImage;
            case "Fonts":
                var isFont = IsFontExtension(extension);
                _logger.LogDebug("验证字体扩展名: fileName={FileName}, extension={Extension}, isValid={IsValid}", fileName, extension, isFont);
                return isFont;
            default:
                _logger.LogDebug("验证资源类型: fileName={FileName}, resourceType={ResourceType}, isValid={IsValid}", fileName, resourceType, true);
                return true;
        }
    }

    /// <summary>
    /// 检查是否为图片扩展名
    /// </summary>
    private bool IsImageExtension(string extension)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        return imageExtensions.Contains(extension);
    }

    /// <summary>
    /// 检查是否为字体扩展名
    /// </summary>
    private bool IsFontExtension(string extension)
    {
        var fontExtensions = new[] { ".ttf", ".otf", ".ttc" };
        return fontExtensions.Contains(extension);
    }
}