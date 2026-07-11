# Setup & Installation

This guide covers a complete local development setup for the backend API, the
SQL Server database, and the React frontend.

## Prerequisites

| Tool | Version | Notes |
| --- | --- | --- |
| .NET SDK | 8.0 | `dotnet --version` should print `8.x` |
| Node.js | 20+ | Includes npm; used for the frontend |
| SQL Server | 2019 / 2022 | Local install or Docker container |
| Docker | Optional | Simplest way to run SQL Server |

## Repository layout

```
GameCenter.sln            Solution file
src/
  GameCenter.Domain        Entities and enums (no dependencies)
  GameCenter.Application    Interfaces, DTOs, and services
  GameCenter.Infrastructure EF Core DbContext, repositories, security, seeding
  GameCenter.Api            ASP.NET Core Web API (controllers, JWT, Swagger)
tests/
  GameCenter.Tests          xUnit tests (EF Core InMemory provider)
frontend/                  React + Vite + TypeScript client
docs/                      Project documentation (this folder)
```

## 1. Database

### Option A — Docker (recommended)

```bash
docker run -d --name gamecenter-mssql \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

Start/stop it later with `docker start gamecenter-mssql` / `docker stop gamecenter-mssql`.

### Option B — Existing SQL Server

Update the `DefaultConnection` connection string in
`src/GameCenter.Api/appsettings.json` to point at your server. The default is:

```
Server=localhost,1433;Database=GameCenterDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true
```

The schema is created with `EnsureCreatedAsync` on startup (no manual migrations
required). To reset data, drop the `GameCenterDb` database and restart the API.

## 2. Backend API

```bash
cd src/GameCenter.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5000
```

Setting `ASPNETCORE_ENVIRONMENT=Development` enables:

- **Swagger UI** at `/swagger`
- **Data seeding** (`SeedData=true` in `appsettings.Development.json`)

### Configuration keys (`appsettings.json`)

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:Key` | JWT signing settings |
| `Cors:AllowedOrigins` | Origins allowed to call the API (default `http://localhost:5173`) |
| `SeedData` | When `true`, seeds sample data on startup |

## 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

The dev server runs on <http://localhost:5173>. The API base URL defaults to
`http://localhost:5000/api` and can be overridden with the `VITE_API_BASE_URL`
environment variable (e.g. in a `frontend/.env` file).

## 4. Verify the toolchain

| Task | Command |
| --- | --- |
| Build backend | `dotnet build GameCenter.sln` |
| Run backend tests | `dotnet test GameCenter.sln` |
| Lint frontend | `npm run lint` (from `frontend/`) |
| Build frontend | `npm run build` (from `frontend/`) |

## Troubleshooting

- **Login fails / no data** — the API may not have seeded. Confirm it started with
  `ASPNETCORE_ENVIRONMENT=Development` and that SQL Server is reachable.
- **CORS errors in the browser** — make sure the frontend origin matches
  `Cors:AllowedOrigins`, and that the API is on port 5000.
- **Cannot connect to SQL Server** — verify the container is up
  (`docker ps`) and the password matches the connection string.
