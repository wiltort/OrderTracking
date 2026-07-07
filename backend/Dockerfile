FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/OrderTracking.Domain/OrderTracking.Domain.csproj src/OrderTracking.Domain/
COPY src/OrderTracking.Application/OrderTracking.Application.csproj src/OrderTracking.Application/
COPY src/OrderTracking.Infrastructure/OrderTracking.Infrastructure.csproj src/OrderTracking.Infrastructure/
COPY src/OrderTracking.API/OrderTracking.API.csproj src/OrderTracking.API/
COPY tests/OrderTracking.UnitTests/OrderTracking.UnitTests.csproj tests/OrderTracking.UnitTests/
COPY tests/OrderTracking.IntegrationTests/OrderTracking.IntegrationTests.csproj tests/OrderTracking.IntegrationTests/
COPY OrderTracking.sln .

RUN dotnet restore

COPY . .

WORKDIR /src/src/OrderTracking.API
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "OrderTracking.API.dll"]