using System.IO;
using System.Drawing;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

/// <summary>
/// 资源验证器，用于验证资源的有效性
/// </summary>
public class ResourceValidator
{
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
                return true;
            }
            
            // 重置流位置
            stream.Position = position;
            return false;
        }
        catch
        {
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
            return stream.Length > 0;
        }
        catch
        {
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
                return IsImageExtension(extension);
            case "Fonts":
                return IsFontExtension(extension);
            default:
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