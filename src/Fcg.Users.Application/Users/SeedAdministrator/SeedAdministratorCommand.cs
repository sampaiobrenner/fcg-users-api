using MediatR;

namespace Fcg.Users.Application.Users.SeedAdministrator;

/// <summary>Garante o administrador padrao da plataforma (executado no startup, apos as migrations).</summary>
public sealed record SeedAdministratorCommand : IRequest<bool>
{
    public const string DefaultName = "Administrador FCG";
    public const string DefaultEmail = "admin@cloudgames.com";
    public const string DefaultPassword = "Admin@123";
}
