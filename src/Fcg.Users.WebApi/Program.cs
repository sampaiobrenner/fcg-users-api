using Fcg.Users.Application;
using Fcg.Users.Domain;
using Fcg.Users.Domain._Shared.Modules;
using Fcg.Users.Infrastructure;
using Fcg.Users.WebApi;
using Fcg.Users.WebApi._Shared.Database;
using Fcg.Users.WebApi._Shared.Endpoints;
using Fcg.Users.WebApi._Shared.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddModule<FcgUsersDomainModule>(builder.Configuration)
    .AddModule<FcgUsersApplicationModule>(builder.Configuration)
    .AddModule<FcgUsersInfrastructureModule>(builder.Configuration)
    .AddModule<FcgUsersWebApiModule>(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync(app.Lifetime.ApplicationStopping);

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthEndpoints();
app.MapEndpoints();

await app.RunAsync();

public partial class Program;
