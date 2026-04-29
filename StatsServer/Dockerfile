FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj отдельно для restore
COPY StatsServer/StatsServer.csproj StatsServer/
RUN dotnet restore StatsServer/StatsServer.csproj

# Копируем всё остальное (включая папку Proto)
COPY . .

WORKDIR /src/StatsServer
RUN dotnet build "StatsServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatsServer.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatsServer.dll"]