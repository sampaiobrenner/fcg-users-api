using MediatR;

namespace Fcg.Users.Application.Users.SeedAdministrator;

/// <summary>
/// Garante o administrador padrao da plataforma (executado no startup, apos as migrations).
/// Os dados vem da configuracao <c>Seed:Administrator</c> (senha no Secret), nunca do codigo.
/// </summary>
public sealed record SeedAdministratorCommand(string Name, string Email, string Password) : IRequest<bool>;
