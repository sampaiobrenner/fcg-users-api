namespace Fcg.Users.IntegrationTests._Fixtures;

/// <summary>Compartilha um unico container PostgreSQL e uma unica API entre as classes de teste.</summary>
[CollectionDefinition(Name)]
public sealed class UsersApiCollection : ICollectionFixture<UsersWebApplicationFactory>
{
    public const string Name = "UsersApi";
}
