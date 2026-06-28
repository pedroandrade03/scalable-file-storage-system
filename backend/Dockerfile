FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/StorageSystem.Api/StorageSystem.Api.csproj src/StorageSystem.Api/
COPY src/StorageSystem.Application/StorageSystem.Application.csproj src/StorageSystem.Application/
COPY src/StorageSystem.Domain/StorageSystem.Domain.csproj src/StorageSystem.Domain/
COPY src/StorageSystem.Infrastructure/StorageSystem.Infrastructure.csproj src/StorageSystem.Infrastructure/

RUN dotnet restore src/StorageSystem.Api/StorageSystem.Api.csproj

COPY src/ src/

RUN dotnet publish src/StorageSystem.Api/StorageSystem.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "StorageSystem.Api.dll"]
