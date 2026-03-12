using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

/// <summary>
    /// 资源扫描器，提供统一的资源扫描功能
    /// </summary>
    public class ResourceScanner
    {
        private readonly IImageCaptchaResourceManager _resourceManager;
    private readonly ResourceValidator _validator;
    private ResourceFilter _filter = ResourceFilter.CreateDefault();
    private readonly ILogger<ResourceScanner> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ResourceScanner(IImageCaptchaResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            _logger = NullLogger<ResourceScanner>.Instance;
            _validator = new ResourceValidator();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ResourceScanner(IImageCaptchaResourceManager resourceManager, ILogger<ResourceScanner>? logger)
        {
            _resourceManager = resourceManager;
            _logger = logger ?? NullLogger<ResourceScanner>.Instance;
            _validator = new ResourceValidator();
        }

        /// <summary>
        /// 将字符串转换为CaptchaType枚举
        /// </summary>
        private CaptchaType StringToCaptchaType(string type)
        {
            if (Enum.TryParse<CaptchaType>(type, true, out var captchaType))
            {
                return captchaType;
            }
            throw new ImageCaptchaException($"Invalid captcha type: {type}");
        }

    /// <summary>
    /// 设置资源过滤器
    /// </summary>
    public ResourceScanner SetFilter(ResourceFilter filter)
    {
        _filter = filter;
        return this;
    }

    /// <summary>
    /// 获取当前资源过滤器
    /// </summary>
    public ResourceFilter GetFilter()
    {
        return _filter;
    }

    /// <summary>
    /// 扫描程序集并添加资源
    /// </summary>
    public void ScanAssembly(Assembly assembly)
    {
        ScanAssembly(assembly, null);
    }

    /// <summary>
    /// 扫描程序集并添加资源
    /// </summary>
    public void ScanAssembly(Assembly assembly, string? prefix)
    {
        var store = _resourceManager.GetResourceStore() as ICrudResourceStore;
        if (store == null) return;

        var resourceNames = assembly.GetManifestResourceNames();
        
        // 如果提供了前缀，过滤资源以提高效率
        if (!string.IsNullOrEmpty(prefix))
        {
            resourceNames = resourceNames.Where(name => name.StartsWith(prefix)).ToArray();
            _logger.LogInformation("使用前缀 {Prefix} 过滤资源，剩余 {Count} 个资源", prefix, resourceNames.Length);
        }

        // 扫描背景图片
        ScanAssemblyBackgroundImages(assembly, resourceNames, store);

        // 扫描模板资源
        ScanAssemblyTemplateResources(assembly, resourceNames, store);

        // 扫描字体资源
        ScanAssemblyFontResources(assembly, resourceNames, store);
    }

    /// <summary>
    /// 扫描目录并添加资源
    /// </summary>
    public void ScanDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new ImageCaptchaException($"Directory not found: {directoryPath}");

        var store = _resourceManager.GetResourceStore() as ICrudResourceStore;
        if (store == null) return;

        // 扫描背景图片
        ScanDirectoryBackgroundImages(directoryPath, store);

        // 扫描模板资源
        ScanDirectoryTemplateResources(directoryPath, store);

        // 扫描字体资源
        ScanDirectoryFontResources(directoryPath, store);
    }

    /// <summary>
    /// 扫描程序集中的背景图片
    /// </summary>
    private void ScanAssemblyBackgroundImages(Assembly assembly, string[] resourceNames, ICrudResourceStore store)
    {
        _logger.LogInformation("开始扫描程序集中的背景图片...");
        foreach (var resourceName in resourceNames)
        {
            // 查找包含 BgImages 的资源
            if (resourceName.Contains("BgImages."))
            {
                _logger.LogDebug("找到背景图片资源: {ResourceName}", resourceName);
                
                // 提取类型和相对路径
                var bgImagesIndex = resourceName.IndexOf("BgImages.") + "BgImages.".Length;
                var parts = resourceName.Substring(bgImagesIndex).Split('.');
                if (parts.Length >= 2)
                {
                    var type = parts[0]; // 转换为大写以匹配 CaptchaType 中的常量
                    // 提取相对路径，保留文件扩展名
                    var relativePath = resourceName.Substring(bgImagesIndex + parts[0].Length + 1);
                    // 只替换路径部分的点，保留文件扩展名
                    var lastDotIndex = relativePath.LastIndexOf(".");
                    if (lastDotIndex > 0)
                    {
                        var pathPart = relativePath.Substring(0, lastDotIndex);
                        var extensionPart = relativePath.Substring(lastDotIndex);
                        relativePath = pathPart.Replace(".", "/") + extensionPart;
                    }
                    _logger.LogDebug("处理背景图片: {Type} - {RelativePath}", type, relativePath);
                    
                    // 验证资源扩展名
                    if (!_validator.ValidateByExtension(relativePath, "BgImages"))
                    {
                        _logger.LogDebug("背景图片扩展名验证失败: {RelativePath}", relativePath);
                        continue;
                    }
                    _logger.LogDebug("背景图片扩展名验证成功: {RelativePath}", relativePath);
                    
                    // 过滤资源
                    if (!_filter.Filter(relativePath))
                    {
                        _logger.LogDebug("背景图片过滤失败: {RelativePath}", relativePath);
                        continue;
                    }
                    _logger.LogDebug("背景图片过滤成功: {RelativePath}", relativePath);
                    
                    // 验证资源有效性
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null && _validator.ValidateImage(stream))
                        {
                            _logger.LogDebug("背景图片有效性验证成功: {ResourceName}", resourceName);
                            var captchaResource = new CaptchaResource
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "embedded",
                                Data = resourceName, // 直接使用完整的资源名称
                                Tag = CommonConstant.DefaultTag,
                                Extra = assembly
                            };
                            
                            // 处理 ALL 类型
                            if (string.Equals(type, "ALL", StringComparison.OrdinalIgnoreCase))
                            {
                                // 添加到所有验证码类型
                                foreach (var captchaType in CaptchaTypeHelper.GetAll())
                                {
                                    store.AddResource(captchaType, captchaResource);
                                    _logger.LogDebug("添加背景图片到 {CaptchaType} 类型: {ResourceName}", captchaType.ToString(), resourceName);
                                }
                            }
                            else
                            {
                                // 处理特定类型
                                // store.AddResource($"BgImages_{type}", captchaResource);
                                var captchaType = StringToCaptchaType(type);
                                store.AddResource(captchaType, captchaResource);
                                _logger.LogDebug("添加背景图片到 {Type} 类型: {ResourceName}", type, resourceName);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("背景图片有效性验证失败: {ResourceName}", resourceName);
                        }
                    }
                }
            }
        }
        _logger.LogInformation("程序集中的背景图片扫描完成");
    }

    /// <summary>
    /// 扫描程序集中的模板资源
    /// </summary>
    private void ScanAssemblyTemplateResources(Assembly assembly, string[] resourceNames, ICrudResourceStore store)
    {
        // 按类型分组模板资源
        var templateGroups = new Dictionary<string, List<(string fileName, string resourceName)>>();
        
        foreach (var resourceName in resourceNames)
        {
            if (resourceName.Contains("Templates."))
            {
                // 提取类型和文件名
                var templatesIndex = resourceName.IndexOf("Templates.") + "Templates.".Length;
                var parts = resourceName.Substring(templatesIndex).Split('.');
                if (parts.Length >= 2)
                {
                    var type = parts[0]; // 转换为大写以匹配 CaptchaType 中的常量
                    // 提取文件名（从类型后面开始，包括所有点）
                    var fileName = resourceName.Substring(templatesIndex + parts[0].Length + 1);
                    
                    // 验证资源扩展名
                    if (!_validator.ValidateByExtension(fileName, "Templates"))
                        continue;
                    
                    // 过滤资源
                    if (!_filter.Filter(fileName))
                        continue;
                    
                    // 验证资源有效性
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null && _validator.ValidateImage(stream))
                        {
                            if (!templateGroups.ContainsKey(type))
                            {
                                templateGroups[type] = new List<(string fileName, string resourceName)>();
                            }
                            templateGroups[type].Add((fileName, resourceName));
                        }
                    }
                }
            }
        }
        
        // 为每个类型创建ResourceMap并添加所有模板资源
        foreach (var group in templateGroups)
        {
            var type = group.Key;
            var templateFiles = group.Value;
            
            // 按模板名称分组（例如：active_1.png和fixed_1.png属于同一组）
            var templateNameGroups = new Dictionary<string, List<(string fileName, string resourceName)>>();
            foreach (var (fileName, resourceName) in templateFiles)
            {
                // 提取模板名称（去除前缀和扩展名）
                var templateName = fileName.StartsWith("active_") ? fileName.Substring(7).Replace(".png", "") : fileName.Substring(6).Replace(".png", "");
                if (!templateNameGroups.ContainsKey(templateName))
                {
                    templateNameGroups[templateName] = new List<(string fileName, string resourceName)>();
                }
                templateNameGroups[templateName].Add((fileName, resourceName));
            }
            
            // 为每个模板组创建ResourceMap
            foreach (var templateNameGroup in templateNameGroups)
            {
                var resourceMap = new ResourceMap
                {
                    Id = Guid.NewGuid().ToString(),
                    Tag = CommonConstant.DefaultTag
                };
                
                // 添加所有模板资源到同一个ResourceMap
                foreach (var (fileName, resourceName) in templateNameGroup.Value)
                {
                    string resourceKey;
                    if (fileName.StartsWith("active_"))
                        resourceKey = "active.png";
                    else if (fileName.StartsWith("fixed_"))
                        resourceKey = "fixed.png";
                    else if (fileName.StartsWith("mask_"))
                        resourceKey = "mask.png";
                    else
                        continue; // 跳过未知模板类型
                    
                    resourceMap.Put(resourceKey, new CaptchaResource
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "embedded",
                        Data = resourceName, // 直接使用完整的资源名称
                        Tag = CommonConstant.DefaultTag,
                        Extra = assembly
                    });
                }
                
                // 处理 ALL 类型
                if (string.Equals(type, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    // 添加到所有验证码类型
                    foreach (var captchaType in CaptchaTypeHelper.GetAll())
                    {
                        store.AddTemplate(captchaType, resourceMap);
                    }
                }
                else
                {
                    // 处理特定类型
                    var captchaType = StringToCaptchaType(type);
                    store.AddTemplate(captchaType, resourceMap);
                }
            }
        }
    }

    /// <summary>
    /// 扫描程序集中的字体资源
    /// </summary>
    private void ScanAssemblyFontResources(Assembly assembly, string[] resourceNames, ICrudResourceStore store)
    {
        foreach (var resourceName in resourceNames)
        {
            if (resourceName.Contains("Fonts"))
            {
                // 提取文件名
                var fileName = resourceName.Substring(resourceName.LastIndexOf('.') + 1);
                
                // 验证资源扩展名
                if (!_validator.ValidateByExtension(fileName, "Fonts"))
                    continue;
                
                // 过滤资源
                if (!_filter.Filter(fileName))
                    continue;
                
                // 验证资源有效性
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null && _validator.ValidateFont(stream))
                    {
                        var captchaResource = new CaptchaResource
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "embedded",
                            Data = resourceName, // 直接使用完整的资源名称
                            Tag = CommonConstant.DefaultTag,
                            Extra = assembly
                        };
                        store.AddFontResource("Fonts", captchaResource);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 扫描目录中的背景图片
    /// </summary>
    private void ScanDirectoryBackgroundImages(string directoryPath, ICrudResourceStore store)
    {
        var bgImagesPath = Path.Combine(directoryPath, "BgImages");
        if (Directory.Exists(bgImagesPath))
        {
            // 扫描所有子目录
            var typeDirectories = Directory.GetDirectories(bgImagesPath);
            foreach (var typeDir in typeDirectories)
            {
                var type = Path.GetFileName(typeDir); // 转换为大写以匹配 CaptchaType 中的常量
                var files = Directory.GetFiles(typeDir, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var relativePath = file.Substring(typeDir.Length + 1).Replace('\\', '/');
                    
                    // 验证资源扩展名
                    if (!_validator.ValidateByExtension(file, "BgImages"))
                        continue;
                    
                    // 过滤资源
                    if (!_filter.Filter(relativePath, new FileInfo(file).Length))
                        continue;
                    
                    // 验证资源有效性
                    using (var stream = File.OpenRead(file))
                    {
                        if (_validator.ValidateImage(stream))
                        {
                            var captchaResource = new CaptchaResource
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "file",
                                Data = file, // Use full absolute path
                                Tag = CommonConstant.DefaultTag
                            };
                            
                            // 处理 ALL 类型
                            if (string.Equals(type, "ALL", StringComparison.OrdinalIgnoreCase))
                            {
                                // 添加到所有验证码类型
                                foreach (var captchaType in CaptchaTypeHelper.GetAll())
                                {
                                    store.AddResource(captchaType, captchaResource);
                                }
                            }
                            else
                            {
                                // 处理特定类型
                                // store.AddResource($"BgImages_{type}", captchaResource);
                                var captchaType = StringToCaptchaType(type);
                                store.AddResource(captchaType, captchaResource);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 扫描目录中的模板资源
    /// </summary>
    private void ScanDirectoryTemplateResources(string directoryPath, ICrudResourceStore store)
    {
        var templatesPath = Path.Combine(directoryPath, "Templates");
        if (Directory.Exists(templatesPath))
        {
            // 扫描所有子目录
            var typeDirectories = Directory.GetDirectories(templatesPath);
            foreach (var typeDir in typeDirectories)
            {
                var type = Path.GetFileName(typeDir); // 转换为大写以匹配 CaptchaType 中的常量
                var files = Directory.GetFiles(typeDir, "*.*", SearchOption.AllDirectories);
                
                // 按模板名称分组（例如：active_1.png和fixed_1.png属于同一组）
                var templateNameGroups = new Dictionary<string, List<(string file, string relativePath)>>();
                foreach (var file in files)
                {
                    var relativePath = file.Substring(typeDir.Length + 1).Replace('\\', '/');
                    
                    // 验证资源扩展名
                    if (!_validator.ValidateByExtension(file, "Templates"))
                        continue;
                    
                    // 过滤资源
                    if (!_filter.Filter(relativePath, new FileInfo(file).Length))
                        continue;
                    
                    // 验证资源有效性
                    using (var stream = File.OpenRead(file))
                    {
                        if (_validator.ValidateImage(stream))
                        {
                            var fileName = Path.GetFileName(file);
                            // 提取模板名称（去除前缀和扩展名）
                            var templateName = fileName.StartsWith("active_") ? fileName.Substring(7).Replace(".png", "") : fileName.Substring(6).Replace(".png", "");
                            if (!templateNameGroups.ContainsKey(templateName))
                            {
                                templateNameGroups[templateName] = new List<(string file, string relativePath)>();
                            }
                            templateNameGroups[templateName].Add((file, relativePath));
                        }
                    }
                }
                
                // 为每个模板组创建ResourceMap
                foreach (var templateNameGroup in templateNameGroups)
                {
                    var resourceMap = new ResourceMap
                    {
                        Id = Guid.NewGuid().ToString(),
                        Tag = CommonConstant.DefaultTag
                    };
                    
                    // 添加所有模板资源到同一个ResourceMap
                foreach (var (file, relativePath) in templateNameGroup.Value)
                {
                    var fileName = Path.GetFileName(file);
                    string resourceKey;
                    if (fileName.StartsWith("active_"))
                        resourceKey = "active.png";
                    else if (fileName.StartsWith("fixed_"))
                        resourceKey = "fixed.png";
                    else if (fileName.StartsWith("mask_"))
                        resourceKey = "mask.png";
                    else
                        continue; // 跳过未知模板类型
                    
                    resourceMap.Put(resourceKey, new CaptchaResource
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "file",
                        Data = file, // Use full absolute path
                        Tag = CommonConstant.DefaultTag
                    });
                }
                    
                    // 处理 ALL 类型
                    if (string.Equals(type, "ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        // 添加到所有验证码类型
                        foreach (var captchaType in CaptchaTypeHelper.GetAll())
                        {
                            store.AddTemplate(captchaType, resourceMap);
                        }
                    }
                    else
                    {
                        // 处理特定类型
                        var captchaType = StringToCaptchaType(type);
                        store.AddTemplate(captchaType, resourceMap);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 扫描目录中的字体资源
    /// </summary>
    private void ScanDirectoryFontResources(string directoryPath, ICrudResourceStore store)
    {
        var fontsPath = Path.Combine(directoryPath, "Fonts");
        if (Directory.Exists(fontsPath))
        {
            var files = Directory.GetFiles(fontsPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relativePath = file.Substring(fontsPath.Length + 1).Replace('\\', '/');
            
            // 验证资源扩展名
            if (!_validator.ValidateByExtension(file, "Fonts"))
                continue;
            
            // 过滤资源
            if (!_filter.Filter(relativePath, new FileInfo(file).Length))
                continue;
            
            // 验证资源有效性
            using (var stream = File.OpenRead(file))
            {
                if (_validator.ValidateFont(stream))
                {
                    var captchaResource = new CaptchaResource
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "file",
                        Data = file, // Use full absolute path
                        Tag = CommonConstant.DefaultTag
                    };
                    store.AddFontResource("Fonts", captchaResource);
                }
            }
            }
        }
    }
}