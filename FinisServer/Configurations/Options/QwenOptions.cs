using System.ComponentModel.DataAnnotations;

namespace FinisServer.Configurations.Options;

public class QwenOptions
{
    public const string SectionName = "Qwen";

    [Required(ErrorMessage = $"{SectionName}: ApiKey 缺失")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = $"{SectionName}: ApiKey 缺失")]
    public string EndpointUrl { get; set; } = string.Empty;
}