using System.Collections.Concurrent;
using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Resource.Impl;

public class InMemoryResourceStore : ICrudResourceStore
{
    private readonly ConcurrentDictionary<CaptchaType, ConcurrentDictionary<string, ConcurrentBag<CaptchaResource>>> _backgroundResourceMap = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentBag<CaptchaResource>>> _fontResourceMap = new();
    private readonly ConcurrentDictionary<CaptchaType, ConcurrentDictionary<string, ConcurrentBag<ResourceMap>>> _templateMap = new();
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public void Init(IImageCaptchaResourceManager resourceManager) { }

    /// <summary>
    /// 添加背景资源
    /// </summary>
    public void AddBackgroundResource(CaptchaType type, CaptchaResource resource)
    {
        var tag = resource.Tag ?? CommonConstant.DefaultTag;
        var tagMap = _backgroundResourceMap.GetOrAdd(type, _ => new ConcurrentDictionary<string, ConcurrentBag<CaptchaResource>>());
        var list = tagMap.GetOrAdd(tag, _ => new ConcurrentBag<CaptchaResource>());
        list.Add(resource);
        // 清除相关缓存
        _cache.TryRemove($"background_{type}_{tag}", out _);
    }

    /// <summary>
    /// 添加字体资源
    /// </summary>
    public void AddFontResource(string type, CaptchaResource resource)
    {
        var tag = resource.Tag ?? CommonConstant.DefaultTag;
        var tagMap = _fontResourceMap.GetOrAdd(type, _ => new ConcurrentDictionary<string, ConcurrentBag<CaptchaResource>>());
        var list = tagMap.GetOrAdd(tag, _ => new ConcurrentBag<CaptchaResource>());
        list.Add(resource);
        // 清除相关缓存
        _cache.TryRemove($"font_{type}_{tag}", out _);
    }

    /// <summary>
    /// 添加模板资源
    /// </summary>
    public void AddTemplateResource(CaptchaType type, ResourceMap template)
    {
        var tag = template.Tag ?? CommonConstant.DefaultTag;
        var tagMap = _templateMap.GetOrAdd(type, _ => new ConcurrentDictionary<string, ConcurrentBag<ResourceMap>>());
        var list = tagMap.GetOrAdd(tag, _ => new ConcurrentBag<ResourceMap>());
        list.Add(template);
        // 清除相关缓存
        _cache.TryRemove($"template_{type}_{tag}", out _);
    }

    public void AddResource(CaptchaType type, CaptchaResource resource)
    {
        // 保持向后兼容，默认添加背景资源
        AddBackgroundResource(type, resource);
    }

    public void AddResource(string type, CaptchaResource resource)
    {
        // 对于字符串类型的资源，默认为字体资源
        AddFontResource(type, resource);
    }

    public void AddTemplate(CaptchaType type, ResourceMap template)
    {
        // 保持向后兼容，默认添加模板资源
        AddTemplateResource(type, template);
    }

    /// <summary>
    /// 删除背景资源
    /// </summary>
    public CaptchaResource? DeleteBackgroundResource(CaptchaType type, string id)
    {
        if (!_backgroundResourceMap.TryGetValue(type, out var tagMap)) return null;
        foreach (var (tag, list) in tagMap)
        {
            var items = list.ToList();
            var item = items.FirstOrDefault(r => r.Id == id);
            if (item != null)
            {
                // 由于ConcurrentBag不支持直接删除，我们需要创建一个新的ConcurrentBag
                var newList = new ConcurrentBag<CaptchaResource>(items.Where(r => r.Id != id));
                tagMap.TryUpdate(tag, newList, list);
                // 清除相关缓存
                _cache.TryRemove($"background_{type}_{tag}", out _);
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// 删除字体资源
    /// </summary>
    public CaptchaResource? DeleteFontResource(string type, string id)
    {
        if (!_fontResourceMap.TryGetValue(type, out var tagMap)) return null;
        foreach (var (tag, list) in tagMap)
        {
            var items = list.ToList();
            var item = items.FirstOrDefault(r => r.Id == id);
            if (item != null)
            {
                // 由于ConcurrentBag不支持直接删除，我们需要创建一个新的ConcurrentBag
                var newList = new ConcurrentBag<CaptchaResource>(items.Where(r => r.Id != id));
                tagMap.TryUpdate(tag, newList, list);
                // 清除相关缓存
                _cache.TryRemove($"font_{type}_{tag}", out _);
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// 删除模板资源
    /// </summary>
    public ResourceMap? DeleteTemplateResource(CaptchaType type, string id)
    {
        if (!_templateMap.TryGetValue(type, out var tagMap)) return null;
        foreach (var (tag, list) in tagMap)
        {
            var items = list.ToList();
            var item = items.FirstOrDefault(r => r.Id == id);
            if (item != null)
            {
                // 由于ConcurrentBag不支持直接删除，我们需要创建一个新的ConcurrentBag
                var newList = new ConcurrentBag<ResourceMap>(items.Where(r => r.Id != id));
                tagMap.TryUpdate(tag, newList, list);
                // 清除相关缓存
                _cache.TryRemove($"template_{type}_{tag}", out _);
                return item;
            }
        }
        return null;
    }

    public CaptchaResource? DeleteResource(CaptchaType type, string id)
    {
        // 保持向后兼容，默认删除背景资源
        return DeleteBackgroundResource(type, id);
    }

    public CaptchaResource? DeleteResource(string type, string id)
    {
        // 对于字符串类型的资源，默认为字体资源
        return DeleteFontResource(type, id);
    }

    public ResourceMap? DeleteTemplate(CaptchaType type, string id)
    {
        // 保持向后兼容，默认删除模板资源
        return DeleteTemplateResource(type, id);
    }

    /// <summary>
    /// 列出背景资源
    /// </summary>
    public List<CaptchaResource> ListBackgroundResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        var effectiveTag = tag ?? CommonConstant.DefaultTag;
        var cacheKey = $"background_{type}_{effectiveTag}";

        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out var cachedList) && cachedList is List<CaptchaResource> resources)
        {
            return resources;
        }

        if (!_backgroundResourceMap.TryGetValue(type, out var tagMap))
        {
            var emptyList = new List<CaptchaResource>();
            _cache.TryAdd(cacheKey, emptyList);
            return emptyList;
        }

        var list = tagMap.TryGetValue(effectiveTag, out var resourcesList) ? resourcesList.ToList() : new List<CaptchaResource>();
        _cache.TryAdd(cacheKey, list);
        return list;
    }

    /// <summary>
    /// 列出字体资源
    /// </summary>
    public List<CaptchaResource> ListFontResourcesByTypeAndTag(string type, string? tag)
    {
        var effectiveTag = tag ?? CommonConstant.DefaultTag;
        var cacheKey = $"font_{type}_{effectiveTag}";

        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out var cachedList) && cachedList is List<CaptchaResource> resources)
        {
            return resources;
        }

        if (!_fontResourceMap.TryGetValue(type, out var tagMap))
        {
            var emptyList = new List<CaptchaResource>();
            _cache.TryAdd(cacheKey, emptyList);
            return emptyList;
        }

        var list = tagMap.TryGetValue(effectiveTag, out var resourcesList) ? resourcesList.ToList() : new List<CaptchaResource>();
        _cache.TryAdd(cacheKey, list);
        return list;
    }

    /// <summary>
    /// 列出模板资源
    /// </summary>
    public List<ResourceMap> ListTemplateResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        var effectiveTag = tag ?? CommonConstant.DefaultTag;
        var cacheKey = $"template_{type}_{effectiveTag}";

        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out var cachedList) && cachedList is List<ResourceMap> templates)
        {
            return templates;
        }

        if (!_templateMap.TryGetValue(type, out var tagMap))
        {
            var emptyList = new List<ResourceMap>();
            _cache.TryAdd(cacheKey, emptyList);
            return emptyList;
        }

        var list = tagMap.TryGetValue(effectiveTag, out var templatesList) ? templatesList.ToList() : new List<ResourceMap>();
        _cache.TryAdd(cacheKey, list);
        return list;
    }

    public List<CaptchaResource> ListResourcesByTypeAndTag(CaptchaType type, string? tag)
    {
        // 保持向后兼容，默认列出背景资源
        return ListBackgroundResourcesByTypeAndTag(type, tag);
    }

    public List<CaptchaResource> ListResourcesByTypeAndTag(string type, string? tag)
    {
        // 对于字符串类型的资源，默认为字体资源
        return ListFontResourcesByTypeAndTag(type, tag);
    }

    public List<ResourceMap> ListTemplatesByTypeAndTag(CaptchaType type, string? tag)
    {
        // 保持向后兼容，默认列出模板资源
        return ListTemplateResourcesByTypeAndTag(type, tag);
    }

    /// <summary>
    /// 随机获取背景资源
    /// </summary>
    public List<CaptchaResource> RandomGetBackgroundResourceByTypeAndTag(CaptchaType type, string? tag, int quantity)
    {
        var all = ListBackgroundResourcesByTypeAndTag(type, tag);
        if (all.Count == 0) return new();
        return RandomPick(all, quantity);
    }

    /// <summary>
    /// 随机获取字体资源
    /// </summary>
    public List<CaptchaResource> RandomGetFontResourceByTypeAndTag(string type, string? tag, int quantity)
    {
        var all = ListFontResourcesByTypeAndTag(type, tag);
        if (all.Count == 0) return new();
        return RandomPick(all, quantity);
    }

    /// <summary>
    /// 随机获取模板资源
    /// </summary>
    public List<ResourceMap> RandomGetTemplateResourceByTypeAndTag(CaptchaType type, string? tag, int quantity)
    {
        var all = ListTemplateResourcesByTypeAndTag(type, tag);
        if (all.Count == 0) return new();
        return RandomPick(all, quantity);
    }

    public List<CaptchaResource> RandomGetResourceByTypeAndTag(CaptchaType type, string? tag, int quantity)
    {
        // 保持向后兼容，默认随机获取背景资源
        return RandomGetBackgroundResourceByTypeAndTag(type, tag, quantity);
    }

    public List<ResourceMap> RandomGetTemplateByTypeAndTag(CaptchaType type, string? tag, int quantity)
    {
        // 保持向后兼容，默认随机获取模板资源
        return RandomGetTemplateResourceByTypeAndTag(type, tag, quantity);
    }

    public void ClearAllResources()
    {
        _backgroundResourceMap.Clear();
        _fontResourceMap.Clear();
        // 清除所有资源缓存
        ClearCacheByPrefix("background_");
        ClearCacheByPrefix("font_");
    }

    public void ClearAllTemplates()
    {
        _templateMap.Clear();
        // 清除所有模板缓存
        ClearCacheByPrefix("template_");
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearAllCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// 清除指定前缀的缓存
    /// </summary>
    private void ClearCacheByPrefix(string prefix)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix)).ToList())
        {
            _cache.TryRemove(key, out _);
        }
    }

    private static List<T> RandomPick<T>(List<T> source, int quantity)
    {
        if (source.Count <= quantity) return new(source);
        var random = Random.Shared;
        var result = new List<T>(quantity);
        var indices = new HashSet<int>();
        while (indices.Count < quantity)
            indices.Add(random.Next(source.Count));
        foreach (var i in indices)
            result.Add(source[i]);
        return result;
    }
}
