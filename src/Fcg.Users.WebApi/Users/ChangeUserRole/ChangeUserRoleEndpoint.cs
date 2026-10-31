using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.ChangeUserRole;
using Fcg.Users.Domain.Users;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Users.WebApi.Users.ChangeUserRole;

internal sealed class ChangeUserRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiRoutes.Users}/{{id:guid}}/role", async (Guid id, ChangeUserRoleRequest request, ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new ChangeUserRoleCommand(id, request.Role), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(ApiRoutes.UsersTag)
            .WithName("ChangeUserRole")
            .Produces<UserResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record ChangeUserRoleRequest(UserRole Role);
