using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.GetUserById;
using Fcg.Users.UnitTests._Fixtures;

namespace Fcg.Users.UnitTests.Application.Users.GetUserById;

public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarUsuario_QuandoExiste()
    {
        await using var context = InMemoryUsersDbContext.Create();
        var user = UserFactory.Create();
        context.Add(user.PersistenceModel);
        await context.SaveChangesAsync();

        var response = await new GetUserByIdQueryHandler(context, new UserResponseParser())
            .Handle(new GetUserByIdQuery(user.Id()), CancellationToken.None);

        response.Should().BeEquivalentTo(new
        {
            Id = user.Id(),
            Name = "Ada Lovelace",
            Email = "ada@fcg.dev",
            Active = true
        });
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoInexistente()
    {
        await using var context = InMemoryUsersDbContext.Create();

        var act = () => new GetUserByIdQueryHandler(context, new UserResponseParser())
            .Handle(new GetUserByIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
