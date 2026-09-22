using Fcg.Users.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.WebApi._Shared.Database;

public static class MigrationExtensions
{
    private const string ApplyMigrationsOnStartupKey = "Database:ApplyMigrationsOnStartup";

    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken)
    {
        if (!app.Configuration.GetValue<bool>(ApplyMigrationsOnStartupKey))
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
