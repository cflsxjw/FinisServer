using FinisServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinisServer.Configurations.Database.Interceptors;

public class TimeInterceptor : SaveChangesInterceptor
{
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