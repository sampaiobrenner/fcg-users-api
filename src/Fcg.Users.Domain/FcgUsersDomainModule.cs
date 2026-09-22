using Fcg.Users.Domain._Shared.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.Domain;

public sealed class FcgUsersDomainModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
