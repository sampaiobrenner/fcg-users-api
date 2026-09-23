using System.Text.Json.Serialization;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Domain._Shared.Modules;
using Fcg.Users.Infrastructure._Shared.Context;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.Errors;
using Fcg.Users.WebApi._Shared.HealthChecks;
using Fcg.Users.WebApi._Shared.Messaging;
using Fcg.Users.WebApi._Shared.Security;

namespace Fcg.Users.WebApi;

public sealed class FcgUsersWebApiModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddJwtAuthentication(configuration);
        services.AddMessaging(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<UsersDbContext>("postgres", tags: [HealthCheckTags.Ready]);

        services.AddEndpoints(typeof(FcgUsersWebApiModule).Assembly);
    }
}
