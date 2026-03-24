using System.ComponentModel.DataAnnotations;

namespace FinisServer.Configurations.Options;

public class PostgresOptions
{
    public const string SectionName = "Postgres";
    
    [Required(ErrorMessage = $"{SectionName}: ConnectionString 缺失")]
    public string ConnectionString { get; set; } = string.Empty;
}