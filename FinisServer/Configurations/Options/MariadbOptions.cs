using System.ComponentModel.DataAnnotations;

namespace FinisServer.Configurations.Options;

public class MariadbOptions
{
    public const string SectionName = "Mariadb";
    
    [Required(ErrorMessage = $"{SectionName}: ConnectionString 缺失")]
    public string ConnectionString { get; set; } = string.Empty;
}