using System.ComponentModel.DataAnnotations;

namespace FinisServer.Configurations.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";
    
    [Required(ErrorMessage = $"{SectionName}: ConnectionString 缺失")]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Required(ErrorMessage = $"{SectionName}: InstanceName 缺失")]
    public string InstanceName { get; set; } = string.Empty;
}