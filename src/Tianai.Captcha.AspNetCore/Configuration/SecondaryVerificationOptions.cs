namespace Tianai.Captcha.AspNetCore.Configuration;

public class SecondaryVerificationOptions
{
    public bool Enabled { get; set; }
    public long Expire { get; set; } = 120000;
    public string KeyPrefix { get; set; } = "captcha:secondary";
}