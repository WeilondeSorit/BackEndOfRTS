```markdown
# BackEndOfRTS

Multiplayer game backend built with ASP.NET Core 8 microservices and PostgreSQL.

## Components

- `MainServer` – core game logic and matchmaking
- `SessionServer` – player session management with Redis
- `StatsServer` – player statistics and leaderboards
- `Proto` – gRPC protocol definitions
- `Shared` – common code shared across services

## Tech Stack

- C# / .NET 8
- PostgreSQL 15
- Redis 7
- gRPC
- Docker

## Database

The `init.sql` script creates tables for players, shop items, matches, units, buildings, and achievements.

## Running

### Docker Compose (recommended)

```bash
docker-compose up -d
```

This starts PostgreSQL, Redis, and all three services.

### Manual

1. Install .NET 8 SDK
2. Start PostgreSQL and Redis
3. Run each service:

```bash
cd MainServer && dotnet run
cd StatsServer && dotnet run
cd SessionServer && dotnet run
```

## Service Ports

| Service | Port |
|---------|------|
| MainServer | 8080 |
| StatsServer | 8081 |
| SessionServer | 8082 |
| PostgreSQL | 5432 |
| Redis | 6379 |
```
