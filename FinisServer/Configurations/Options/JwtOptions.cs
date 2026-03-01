using System.ComponentModel.DataAnnotations;

namespace FinisServer.Configurations.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = $"{SectionName}: SecretKey 缺失")]
    [MinLength(16, ErrorMessage = "密钥长度至少需要16位")]
    public string SecretKey { get; set; } = string.Empty;
    
    [Required(ErrorMessage = $"{SectionName}: Issuer 缺失")] 
    public string Issuer { get; set; } = string.Empty;
    
    [Required(ErrorMessage = $"{SectionName}: Audience 缺失")] 
    public string Audience { get; set; } = string.Empty;
    
    [Required(ErrorMessage = $"{SectionName}: ExpiryMinutes 缺失")]
    public int ExpiryMinutes { get; set; } = 0;
}