FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json nuget.config Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY src/Fcg.Users.Domain/Fcg.Users.Domain.csproj src/Fcg.Users.Domain/
COPY src/Fcg.Users.Application/Fcg.Users.Application.csproj src/Fcg.Users.Application/
COPY src/Fcg.Users.Infrastructure/Fcg.Users.Infrastructure.csproj src/Fcg.Users.Infrastructure/
COPY src/Fcg.Users.WebApi/Fcg.Users.WebApi.csproj src/Fcg.Users.WebApi/
RUN dotnet restore src/Fcg.Users.WebApi/Fcg.Users.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/Fcg.Users.WebApi/Fcg.Users.WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Fcg.Users.WebApi.dll"]
