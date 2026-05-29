using System.Text.Json.Serialization;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Dtos;

using System.Text.Json.Serialization;

public record ReportDto(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    ReportType ReportType,
    string Reason,
    int TargetIdentifierId
);