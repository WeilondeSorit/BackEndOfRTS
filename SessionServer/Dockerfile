FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false
# Копируем csproj файлы с сохранением структуры папок
COPY ["SessionServer/SessionServer.csproj", "SessionServer/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "SessionServer/SessionServer.csproj"

COPY . .
WORKDIR "/src/SessionServer"
RUN dotnet build "SessionServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SessionServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SessionServer.dll"]