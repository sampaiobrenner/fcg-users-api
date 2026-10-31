using System.Reflection;
using Fcg.Users.Application._Shared.Behaviors;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Domain._Shared.Modules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fcg.Users.Application;

public sealed class FcgUsersApplicationModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(FcgUsersApplicationModule).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        AddParsers(services, assembly);
    }

    // Registra cada Parser<TIn,TOut> concreto como IParser<TIn,TOut> (sem estado, singleton).
    private static void AddParsers(IServiceCollection services, Assembly assembly)
    {
        var descriptors = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.ImplementedInterfaces
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IParser<,>))
                .Select(contract => ServiceDescriptor.Singleton(contract, type)));

        foreach (var descriptor in descriptors)
            services.TryAdd(descriptor);
    }
}
