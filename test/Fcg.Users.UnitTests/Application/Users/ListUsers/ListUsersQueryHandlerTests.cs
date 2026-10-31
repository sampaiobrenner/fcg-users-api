using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.ListUsers;
using Fcg.Users.UnitTests._Fixtures;

namespace Fcg.Users.UnitTests.Application.Users.ListUsers;

public class ListUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarTodosOrdenadosPorNome_QuandoExistemUsuarios()
    {
        await using var context = InMemoryUsersDbContext.Create();
        context.AddRange(
            UserFactory.Create("Grace Hopper", "grace@fcg.dev").PersistenceModel,
            UserFactory.Create("Ada Lovelace", "ada@fcg.dev").PersistenceModel);
        await context.SaveChangesAsync();

        var response = await new ListUsersQueryHandler(context, new UserResponseParser())
            .Handle(new ListUsersQuery(), CancellationToken.None);

        response.Select(user => user.Name).Should().Equal("Ada Lovelace", "Grace Hopper");
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoNaoExistemUsuarios()
    {
        await using var context = InMemoryUsersDbContext.Create();

        var response = await new ListUsersQueryHandler(context, new UserResponseParser())
            .Handle(new ListUsersQuery(), CancellationToken.None);

        response.Should().BeEmpty();
    }
}
