FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_EnableRegexTimeout=false

COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["MainServer/MainServer.csproj", "MainServer/"]

RUN dotnet restore "MainServer/MainServer.csproj"

COPY . .
WORKDIR "/src/MainServer"
RUN dotnet build "MainServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MainServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MainServer.dll"]