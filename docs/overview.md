# Game Center CRM — Documentation

Welcome to the **Game Center CRM** documentation. This is the central reference for
setting up, understanding, and operating the application.

Game Center CRM is a multi-location management system for game clubs. It tracks
customers, cashiers, transactions (add money / withdraw money / bonus points),
expenses, games, and produces financial reports (profit & loss, balance sheet,
general ledger, cashier cash register).

## What's inside

| Guide | Description |
| --- | --- |
| [Quick Start](quick-start) | Get the app running and post your first transaction in minutes. |
| [Setup & Installation](setup) | Full development environment setup (backend, database, frontend). |
| [Architecture](architecture) | How the system is structured across projects and layers. |
| [User Guide](user-guide) | Roles, screens, and day-to-day workflows. |
| [API Reference](api-reference) | REST endpoints exposed by the backend API. |

## Technology at a glance

- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core, SQL Server.
- **Frontend**: React 19 + TypeScript, Vite, Material UI (MUI).
- **Auth**: JWT bearer tokens with role-based permissions (Admin, Manager, Cashier).

## Default accounts

The development database is seeded with these accounts:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `Admin@12345` | Admin |
| `manager` | `Manager@12345` | Manager |
| `cashier1` | `Cashier@12345` | Cashier |
| `cashier2` | `Cashier@12345` | Cashier |

> These credentials are for local development only. Change them before any real deployment.
