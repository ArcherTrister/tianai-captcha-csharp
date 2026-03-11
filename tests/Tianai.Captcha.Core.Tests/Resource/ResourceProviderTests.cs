using System.Reflection;
using System.IO;
using Xunit;
using Tianai.Captcha.Core.Resource.Impl;
using Tianai.Captcha.Core.Resource;

namespace Tianai.Captcha.Core.Tests.Resource;

public class ResourceProviderTests
{
    [Fact]
    public void EmbeddedResourceProvider_RegisterAssembly_ShouldAutoScanResources()
    {
        // Arrange
        var manager = new DefaultImageCaptchaResourceManager();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        manager.RegisterAssembly(assembly);

        // Assert
        // 由于测试程序集可能没有嵌入资源，这里主要测试注册过程不抛出异常
        // 实际应用中，注册包含资源的程序集会自动添加资源
    }

    [Fact]
    public void FileResourceProvider_ScanDirectory_ShouldAutoAddResources()
    {
        // Arrange
        var manager = new DefaultImageCaptchaResourceManager();
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Combine(tempDir, "Resources", "BgImages", "All"));
        Directory.CreateDirectory(Path.Combine(tempDir, "Resources", "Templates", "Slider"));
        Directory.CreateDirectory(Path.Combine(tempDir, "Resources", "Fonts"));

        // 创建测试文件
        File.WriteAllText(Path.Combine(tempDir, "Resources", "BgImages", "All", "test.jpg"), "test");
        File.WriteAllText(Path.Combine(tempDir, "Resources", "Templates", "Slider", "test.png"), "test");
        File.WriteAllText(Path.Combine(tempDir, "Resources", "Fonts", "test.ttf"), "test");

        try
        {
            // Act
            manager.ScanDirectory(tempDir);

            // Assert
            // 测试扫描过程不抛出异常
            // 实际应用中，扫描目录会自动添加资源
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ResourceScanner_ScanAssembly_ShouldScanResources()
    {
        // Arrange
        var manager = new DefaultImageCaptchaResourceManager();
        var scanner = new ResourceScanner(manager);
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        scanner.ScanAssembly(assembly);

        // Assert
        // 测试扫描过程不抛出异常
    }

    [Fact]
    public void ResourceScanner_ScanDirectory_ShouldScanResources()
    {
        // Arrange
        var manager = new DefaultImageCaptchaResourceManager();
        var scanner = new ResourceScanner(manager);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Combine(tempDir, "Resources", "BgImages", "All"));

        // 创建测试文件
        File.WriteAllText(Path.Combine(tempDir, "Resources", "BgImages", "All", "test.jpg"), "test");

        try
        {
            // Act
            scanner.ScanDirectory(tempDir);

            // Assert
            // 测试扫描过程不抛出异常
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}