using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.GetUserById;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Users.WebApi.Users.GetUserById;

internal sealed class GetUserByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Users}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(ApiRoutes.UsersTag)
            .WithName("GetUserById")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
