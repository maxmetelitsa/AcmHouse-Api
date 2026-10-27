FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so the package layer is cached between builds.
COPY src/AcmHouse.Domain/AcmHouse.Domain.csproj src/AcmHouse.Domain/
COPY src/AcmHouse.Application/AcmHouse.Application.csproj src/AcmHouse.Application/
COPY src/AcmHouse.Infrastructure/AcmHouse.Infrastructure.csproj src/AcmHouse.Infrastructure/
COPY src/AcmHouse.Api/AcmHouse.Api.csproj src/AcmHouse.Api/
RUN dotnet restore src/AcmHouse.Api/AcmHouse.Api.csproj

COPY src/ src/
RUN dotnet publish src/AcmHouse.Api/AcmHouse.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AcmHouse.Api.dll"]
