using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceArchiving.Data;

public class CustomUserIdInterceptor : ISaveChangesInterceptor
{
    private readonly Func<int> _getCurrentUserId;

    // Inject a delegate to get the current user ID
    public CustomUserIdInterceptor(Func<int> getCurrentUserId)
    {
        _getCurrentUserId = getCurrentUserId;
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateUserIds(eventData.Context);
        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateUserIds(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void UpdateUserIds(DbContext? context)
    {
        if (context == null) return;

        var userId = _getCurrentUserId();
        if (userId == 0) return; // No valid user ID

        var entries = context.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.UserId = userId;
            }
        }
    }




}
