using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.UpdateUser;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Users.WebApi.Users.UpdateUser;

internal sealed class UpdateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiRoutes.Users}/{{id:guid}}", async (Guid id, UpdateUserRequest request, ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new UpdateUserCommand(id, request.Name), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(ApiRoutes.UsersTag)
            .WithName("UpdateUser")
            .Produces<UserResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record UpdateUserRequest(string Name);
