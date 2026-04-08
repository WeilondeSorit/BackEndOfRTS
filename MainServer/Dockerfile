FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false
# Копируем csproj файлы с сохранением структуры папок
COPY ["MainServer/MainServer.csproj", "MainServer/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "MainServer/MainServer.csproj"

COPY . .
WORKDIR "/src/MainServer"
RUN dotnet build "MainServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MainServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MainServer.dll"]