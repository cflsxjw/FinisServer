namespace FinisServer.Models.Dtos;
using FinisServer.Models.Enums;

public record ReportInfoDto(
    int Id,
    ReportType ReportType,
    string Reason,
    int TargetIdentifierId);