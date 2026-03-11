namespace Tianai.Captcha.Core.Common;

public class GenerateParam : AnyMap
{
    private static readonly ParamKey<string> BackgroundFormatNameKey = new("backgroundFormatName");
    private static readonly ParamKey<string> TemplateFormatNameKey = new("templateFormatName");
    private static readonly ParamKey<bool> ObfuscateKey = new("obfuscate");
    private static readonly ParamKey<string> TypeKey = new("type");
    private static readonly ParamKey<string> BackgroundImageTagKey = new("backgroundImageTag");
    private static readonly ParamKey<string> TemplateImageTagKey = new("templateImageTag");

    public GenerateParam()
    {
        BackgroundFormatName = "jpeg";
        TemplateFormatName = "png";
        Obfuscate = false;
        Type = CaptchaType.Slider.ToString();
    }

    public string BackgroundFormatName
    {
        get => GetParam(BackgroundFormatNameKey, "jpeg")!;
        set => AddParam(BackgroundFormatNameKey, value);
    }

    public string TemplateFormatName
    {
        get => GetParam(TemplateFormatNameKey, "png")!;
        set => AddParam(TemplateFormatNameKey, value);
    }

    public bool Obfuscate
    {
        get => GetParam(ObfuscateKey);
        set => AddParam(ObfuscateKey, value);
    }

    public string Type
    {
        get => GetParam(TypeKey, CaptchaType.Slider.ToString())!;
        set => AddParam(TypeKey, value);
    }

    public CaptchaType CaptchaType
    {
        get => Enum.Parse<CaptchaType>(GetParam(TypeKey, CaptchaType.Slider.ToString())!);
        set => AddParam(TypeKey, value.ToString());
    }

    public string? BackgroundImageTag
    {
        get => GetParam(BackgroundImageTagKey);
        set => AddParam(BackgroundImageTagKey, value!);
    }

    public string? TemplateImageTag
    {
        get => GetParam(TemplateImageTagKey);
        set => AddParam(TemplateImageTagKey, value!);
    }

    public static GenerateParamBuilder CreateBuilder() => new();

    public class GenerateParamBuilder
    {
        private string _backgroundFormatName = "jpeg";
        private string _templateFormatName = "png";
        private bool _obfuscate;
        private string _type = CaptchaType.Slider.ToString();
        private string? _backgroundImageTag;
        private string? _templateImageTag;

        public GenerateParamBuilder BackgroundFormatName(string value) { _backgroundFormatName = value; return this; }
        public GenerateParamBuilder TemplateFormatName(string value) { _templateFormatName = value; return this; }
        public GenerateParamBuilder Obfuscate(bool value) { _obfuscate = value; return this; }
        public GenerateParamBuilder Type(string value) { _type = value; return this; }
        public GenerateParamBuilder Type(CaptchaType value) { _type = value.ToString(); return this; }
        public GenerateParamBuilder BackgroundImageTag(string? value) { _backgroundImageTag = value; return this; }
        public GenerateParamBuilder TemplateImageTag(string? value) { _templateImageTag = value; return this; }

        public GenerateParam Build()
        {
            var param = new GenerateParam
            {
                BackgroundFormatName = _backgroundFormatName,
                TemplateFormatName = _templateFormatName,
                Obfuscate = _obfuscate,
                Type = _type,
                BackgroundImageTag = _backgroundImageTag,
                TemplateImageTag = _templateImageTag
            };
            return param;
        }
    }
}
