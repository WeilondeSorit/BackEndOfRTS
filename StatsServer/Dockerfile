FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false
# Копируем csproj файлы с сохранением структуры папок
COPY ["StatsServer/StatsServer.csproj", "StatsServer/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "StatsServer/StatsServer.csproj"

COPY . .
WORKDIR "/src/StatsServer"
RUN dotnet build "StatsServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatsServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatsServer.dll"]