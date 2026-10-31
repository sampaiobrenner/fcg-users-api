using Fcg.Users.Application.Users.DeactivateUser;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Users.WebApi.Users.DeactivateUser;

internal sealed class DeactivateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{ApiRoutes.Users}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeactivateUserCommand(id), cancellationToken);
                return TypedResults.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(ApiRoutes.UsersTag)
            .WithName("DeactivateUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
