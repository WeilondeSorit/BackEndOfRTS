FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false
# Копируем csproj файлы с сохранением структуры папок
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["SessionServer/SessionServer.csproj", "SessionServer/"]
COPY ["StatsServer/StatsServer.csproj", "StatsServer/"]
COPY ["MainServer/MainServer.csproj", "MainServer/"]
RUN dotnet restore "SessionServer/SessionServer.csproj"

COPY . .
WORKDIR "/src/SessionServer"
RUN dotnet build "SessionServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SessionServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SessionServer.dll"]