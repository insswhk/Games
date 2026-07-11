# Quick Start Guide

This guide gets you from a fresh clone to a running application and a first posted
transaction. For a deeper explanation of each step, see the
[Setup & Installation](setup) guide.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (ships with npm)
- A running **SQL Server** instance (Docker is the easiest option)

## 1. Start SQL Server

The API expects SQL Server on `localhost:1433`. The quickest way is Docker:

```bash
docker run -d --name gamecenter-mssql \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

The default connection string lives in `src/GameCenter.Api/appsettings.json`.

## 2. Run the backend API

From the repository root:

```bash
cd src/GameCenter.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5000
```

On first run in the `Development` environment the database schema is created and
seeded automatically (locations, users, customers, sample transactions). Swagger
is available at <http://localhost:5000/swagger>.

> **Tip:** run the API on port **5000** — that is the URL the frontend talks to by default.

## 3. Run the frontend

In a second terminal:

```bash
cd frontend
npm install   # first time only
npm run dev
```

Open <http://localhost:5173>.

## 4. Log in

Use the seeded administrator account:

- **Username:** `admin`
- **Password:** `Admin@12345`

## 5. Post your first transaction

1. In the sidebar, open **Transactions**.
2. Fill in the form:
   - **Location:** Downtown Game Club
   - **Cashier:** CASH-A-001
   - **Customer:** CUS-001
   - **Game Mode:** A · Club Games
   - **Transaction Type:** AddMoney
   - **Shift:** Day
   - **Amount:** 100
3. Click **Post Transaction**. A green banner confirms the updated customer balance
   and cashier register.

That's it — you now have a working Game Center CRM environment. Continue with the
[User Guide](user-guide) to explore the rest of the app.
