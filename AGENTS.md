# Game Center CRM

A multi-location game-center CRM. Two deliverables:

- **Backend**: .NET 8 Web API (`GameCenter.sln`) using EF Core + **SQL Server**. Clean-architecture layout under `src/` (`Domain`, `Application`, `Infrastructure`, `Api`). Tests in `tests/GameCenter.Tests` (xUnit, EF Core **InMemory** provider).
- **Frontend**: React 19 + Vite + TypeScript + MUI in `frontend/` (talks to the API over HTTP, JWT bearer auth).

## Cursor Cloud specific instructions

These notes assume the update script (dependency refresh) has already run. They cover non-obvious startup/run caveats; standard commands live in `frontend/package.json` and the `.csproj`/`.sln` files.

### Required external service: SQL Server
The API connects to SQL Server (`ConnectionStrings:DefaultConnection` in `src/GameCenter.Api/appsettings.json`, `Server=localhost,1433`, user `sa`, password `Your_password123`). It is **not** part of the update script. Run it via Docker (the daemon itself must be started first in this VM — systemd is not running):

```bash
sudo dockerd > /tmp/dockerd.log 2>&1 &        # start Docker daemon (userspace), then wait ~8s
sudo docker start gamecenter-mssql 2>/dev/null \
  || sudo docker run -d --name gamecenter-mssql \
       -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
       -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

Docker is configured with the `fuse-overlayfs` storage driver and `iptables-legacy` (required in this VM).

### Running the API
- The frontend's API client defaults to `http://localhost:5000/api` (`frontend/src/api/client.ts`), but the launch profile (`Properties/launchSettings.json`) listens on `5100`. **Run the API on port 5000** to match the frontend, e.g. from `src/GameCenter.Api`:
  ```bash
  ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5000
  ```
- `ASPNETCORE_ENVIRONMENT=Development` enables Swagger (`/swagger`) and `SeedData=true` (`appsettings.Development.json`), which seeds locations, users, customers, etc. via `EnsureCreatedAsync` on startup. The first run creates the `GameCenterDb` schema; subsequent runs no-op the seed if users already exist.
- Seeded login accounts (username / password): `admin` / `Admin@12345`, `manager` / `Manager@12345`, `cashier1` / `Cashier@12345`, `cashier2` / `Cashier@12345`.
- `EnsureCreatedAsync` does not run migrations. To reset data, drop the DB in the container (e.g. `DROP DATABASE GameCenterDb`) or recreate the container, then restart the API.

### Running the frontend
From `frontend/`: `npm run dev` (Vite on `http://localhost:5173`, the CORS-allowed origin). Override the API base with `VITE_API_BASE_URL` if needed.

### Lint / test / build quick reference
- Backend build: `dotnet build GameCenter.sln`
- Backend tests: `dotnet test GameCenter.sln` (InMemory provider — no SQL Server needed for tests)
- Frontend lint: `npm run lint` (oxlint); frontend build: `npm run build`

### dotnet on PATH
The .NET 8 SDK is installed at `~/.dotnet` and added to PATH via `~/.bashrc`. New non-login shells may need `export PATH="$HOME/.dotnet:$PATH"`.
