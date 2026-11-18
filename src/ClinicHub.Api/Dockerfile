# Build from the repository root:  docker build -f src/ClinicHub.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the dependency layer is cached between code changes.
COPY src/ClinicHub.Domain/ClinicHub.Domain.csproj src/ClinicHub.Domain/
COPY src/ClinicHub.Application/ClinicHub.Application.csproj src/ClinicHub.Application/
COPY src/ClinicHub.Infrastructure/ClinicHub.Infrastructure.csproj src/ClinicHub.Infrastructure/
COPY src/ClinicHub.Api/ClinicHub.Api.csproj src/ClinicHub.Api/
RUN dotnet restore src/ClinicHub.Api/ClinicHub.Api.csproj

COPY src/ src/
RUN dotnet publish src/ClinicHub.Api/ClinicHub.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Run as the image's built-in non-root user.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicHub.Api.dll"]
