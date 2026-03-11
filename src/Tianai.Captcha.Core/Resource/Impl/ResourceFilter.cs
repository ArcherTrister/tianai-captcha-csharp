using System.IO;
using System.Text.RegularExpressions;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

/// <summary>
/// 资源过滤器，用于过滤不需要的资源
/// </summary>
public class ResourceFilter
{
    private readonly List<string> _allowedExtensions = new();
    private readonly List<string> _deniedExtensions = new();
    private readonly List<string> _allowedPatterns = new();
    private readonly List<string> _deniedPatterns = new();
    private long _minSize = 0;
    private long _maxSize = long.MaxValue;

    /// <summary>
    /// 添加允许的文件扩展名
    /// </summary>
    public ResourceFilter AllowExtension(params string[] extensions)
    {
        _allowedExtensions.AddRange(extensions);
        return this;
    }

    /// <summary>
    /// 添加拒绝的文件扩展名
    /// </summary>
    public ResourceFilter DenyExtension(params string[] extensions)
    {
        _deniedExtensions.AddRange(extensions);
        return this;
    }

    /// <summary>
    /// 添加允许的文件名称模式
    /// </summary>
    public ResourceFilter AllowPattern(params string[] patterns)
    {
        _allowedPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// 添加拒绝的文件名称模式
    /// </summary>
    public ResourceFilter DenyPattern(params string[] patterns)
    {
        _deniedPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// 设置文件大小范围
    /// </summary>
    public ResourceFilter SetSizeRange(long minSize, long maxSize)
    {
        _minSize = minSize;
        _maxSize = maxSize;
        return this;
    }

    /// <summary>
    /// 过滤资源
    /// </summary>
    public bool Filter(string fileName, long fileSize = 0)
    {
        // 检查文件扩展名
        var extension = Path.GetExtension(fileName).ToLower();
        if (_deniedExtensions.Count > 0 && _deniedExtensions.Contains(extension))
            return false;
        if (_allowedExtensions.Count > 0 && !_allowedExtensions.Contains(extension))
            return false;

        // 检查文件名称模式
        var fileNameOnly = Path.GetFileName(fileName);
        foreach (var pattern in _deniedPatterns)
        {
            if (Regex.IsMatch(fileNameOnly, pattern))
                return false;
        }
        if (_allowedPatterns.Count > 0)
        {
            bool matched = false;
            foreach (var pattern in _allowedPatterns)
            {
                if (Regex.IsMatch(fileNameOnly, pattern))
                {
                    matched = true;
                    break;
                }
            }
            if (!matched)
                return false;
        }

        // 检查文件大小
        if (fileSize < _minSize || fileSize > _maxSize)
            return false;

        return true;
    }

    /// <summary>
    /// 创建默认过滤器
    /// </summary>
    public static ResourceFilter CreateDefault()
    {
        return new ResourceFilter()
            .AllowExtension(".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ttf", ".otf", ".ttc")
            .SetSizeRange(0, 10 * 1024 * 1024); // 10MB
    }
}