using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.ListUsers;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Users.WebApi.Users.ListUsers;

internal sealed class ListUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Users, async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new ListUsersQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(ApiRoutes.UsersTag)
            .WithName("ListUsers")
            .Produces<IReadOnlyCollection<UserResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
