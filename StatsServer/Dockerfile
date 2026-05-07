FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false

# Копируем только необходимые .csproj
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["StatsServer/StatsServer.csproj", "StatsServer/"]

# Восстанавливаем зависимости (ОБЯЗАТЕЛЬНО)
RUN dotnet restore "StatsServer/StatsServer.csproj"

# Копируем всё остальное
COPY . .

WORKDIR "/src/StatsServer"
RUN dotnet build "StatsServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatsServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatsServer.dll"]