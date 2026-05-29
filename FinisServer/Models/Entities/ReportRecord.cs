using FinisServer.Interfaces;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Entities;

public class ReportRecord : IAuditEntity
{
    public int Id { get; set; }
    public required ReportType ReportType { get; set; }
    public bool IsDone { get; set; } = false;
    public required string Reason { get; set; }
    public int? AdminId { get; set; }
    public required int TargetIdentifierId { get; set; }
    public required int ReporterId { get; set; }
    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
}