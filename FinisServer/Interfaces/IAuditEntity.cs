namespace FinisServer.Interfaces;

public interface IAuditEntity
{
    public DateTimeOffset CreatedTimeOffset { get; set; }
    public DateTimeOffset UpdatedTimeOffset { get; set; }

}