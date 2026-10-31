using Fcg.Users.Application.Auth.Login;
using Fcg.Users.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Users.WebApi.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Auth}/login", async (LoginCommand command, ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(command, cancellationToken)))
            .AllowAnonymous()
            .WithTags(ApiRoutes.AuthTag)
            .WithName("Login")
            .Produces<LoginResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
