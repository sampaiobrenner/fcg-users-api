using Fcg.Users.Application.Users._Shared;
using MediatR;

namespace Fcg.Users.Application.Users.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<UserResponse>;
