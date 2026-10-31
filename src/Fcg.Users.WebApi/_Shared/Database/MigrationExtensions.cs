using Fcg.Users.Application.Users.SeedAdministrator;
using Fcg.Users.Infrastructure._Shared.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fcg.Users.WebApi._Shared.Database;

public static class MigrationExtensions
{
    private const string ApplyMigrationsOnStartupKey = "Database:ApplyMigrationsOnStartup";

    /// <summary>
    /// Aplica as migrations pendentes e, em seguida, garante o administrador padrao (seed idempotente).
    /// Ambos so rodam quando <c>Database:ApplyMigrationsOnStartup</c> esta ligado.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken)
    {
        if (!app.Configuration.GetValue<bool>(ApplyMigrationsOnStartupKey))
            return;

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            await context.Database.MigrateAsync(cancellationToken);
        }

        await SeedAdministratorAsync(app, cancellationToken);
    }

    private static async Task SeedAdministratorAsync(WebApplication app, CancellationToken cancellationToken)
    {
        var options = app.Configuration.GetSection(AdministratorSeedOptions.SectionName).Get<AdministratorSeedOptions>() ?? new AdministratorSeedOptions();

        if (!options.IsConfigured)
        {
            app.Logger.LogWarning("Seed do administrador ignorado: configure Seed__Administrator__Email e Seed__Administrator__Password");
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            await sender.Send(new SeedAdministratorCommand(options.Name, options.Email, options.Password), cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Outra replica criou o administrador ao mesmo tempo: o objetivo do seed ja foi atingido.
            app.Logger.LogInformation("Administrador padrao ja criado por outra instancia");
        }
    }
}
