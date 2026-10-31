using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.RegisterUser;
using Fcg.Users.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Users.WebApi.Users.RegisterUser;

internal sealed class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Auth}/register", async (RegisterUserCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var user = await sender.Send(command, cancellationToken);
                return TypedResults.Created($"{ApiRoutes.Users}/{user.Id}", user);
            })
            .AllowAnonymous()
            .WithTags(ApiRoutes.AuthTag)
            .WithName("RegisterUser")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
