using FinisServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinisServer.Configurations.Database.Interceptors;

public class TimeInterceptor : SaveChangesInterceptor
{
    public bool SuppressAudit { get; set; } = false;
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = context.ChangeTracker.Entries<IAuditEntity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    var silentFields = new[] { "ViewCount", "LastAccessed" };
                    var hasRealChanges = entry.Properties
                        .Any(p => p.IsModified && !silentFields.Contains(p.Metadata.Name));
                    if (hasRealChanges)
                    {
                        entry.Entity.UpdatedTimeOffset = DateTimeOffset.UtcNow;
                    }
                    entry.Entity.CreatedTimeOffset = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedTimeOffset = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedTimeOffset = DateTimeOffset.UtcNow;
                    break;
                default:
                    break;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}