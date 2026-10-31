using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.GetMyProfile;
using Fcg.Users.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Users.WebApi.Users.GetMyProfile;

internal sealed class GetMyProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Users}/me", async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetMyProfileQuery(), cancellationToken)))
            .RequireAuthorization()
            .WithTags(ApiRoutes.UsersTag)
            .WithName("GetMyProfile")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
