using Fcg.Users.Application.Users._Shared;
using Fcg.Users.UnitTests._Fixtures;

namespace Fcg.Users.UnitTests.Application.Users._Shared;

public class UserCreatedEventParserTests
{
    private readonly UserCreatedEventParser _parser = new();

    [Fact]
    public void Parse_DeveMontarEventoDoContrato_QuandoUsuarioInformado()
    {
        var user = UserFactory.Create();

        var integrationEvent = _parser.Parse(user)!;

        integrationEvent.UserId.Should().Be(user.Id());
        integrationEvent.Name.Should().Be("Ada Lovelace");
        integrationEvent.Email.Should().Be("ada@fcg.dev");
        integrationEvent.EventId.Version.Should().Be(7);
        integrationEvent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Parse_DeveGerarEventIdNovo_QuandoChamadoMaisDeUmaVez()
    {
        var user = UserFactory.Create();

        _parser.Parse(user)!.EventId.Should().NotBe(_parser.Parse(user)!.EventId);
    }
}
