# Architecture

Game Center CRM follows a layered (clean-architecture inspired) design on the
backend, with a single-page React application on the frontend.

## High-level view

```
┌────────────────────┐        HTTPS/JSON        ┌───────────────────────────┐
│   React Frontend   │  ───────────────────────▶ │      ASP.NET Core API     │
│  (Vite + MUI)      │  ◀─────────────────────── │   (Controllers + JWT)     │
└────────────────────┘        JWT bearer          └────────────┬──────────────┘
                                                                │
                                                    ┌───────────▼───────────┐
                                                    │  Application services  │
                                                    └───────────┬───────────┘
                                                                │
                                                    ┌───────────▼───────────┐
                                                    │  Infrastructure (EF)   │
                                                    │  + SQL Server database │
                                                    └────────────────────────┘
```

## Backend projects

| Project | Responsibility |
| --- | --- |
| `GameCenter.Domain` | Core entities (`Customer`, `Cashier`, `TransactionRecord`, `LedgerEntry`, …) and enums. No external dependencies. |
| `GameCenter.Application` | Interfaces (`ITransactionService`, `IReportingService`, …), DTOs, request contracts, and business services. |
| `GameCenter.Infrastructure` | EF Core `GameCenterDbContext`, repositories, `UnitOfWork`, password hashing, and data seeding. |
| `GameCenter.Api` | Controllers, JWT authentication/authorization, Swagger, CORS, and dependency injection wiring. |

Dependencies flow inward: `Api → Application → Domain`, with `Infrastructure`
implementing the `Application` interfaces.

## Key domain concepts

- **Location** — a physical game club. Most records are scoped to a location.
- **Cashier** — an operator with a cash register balance, tied to a location.
- **Customer** — a player with a balance and bonus points; may have a referral.
- **Transaction** — an `AddMoney`, `WithdrawMoney`, or `BonusPoints` entry that
  updates customer balances and cashier registers and generates ledger entries.
- **Ledger / Accounts** — double-entry style records powering financial reports.
- **Expense** — operational costs (rent, salary, etc.) posted against accounts.

## Security & permissions

- Authentication uses **JWT bearer** tokens issued by `/api/auth/login`.
- Each user has a **role**: `Admin`, `Manager`, or `Cashier`.
- Per-form permissions (`CanOpen`, `CanAdd`, `CanDelete`, `CanViewReports`) are
  stored in the `Permissions` table and enforced by `IPermissionService`.
- The frontend hides navigation items the current user cannot open; the backend
  independently enforces the same permissions on every request.

## Frontend structure

```
frontend/src/
  api/client.ts        Axios instance + JWT interceptor
  auth/AuthContext.tsx  Login state, permissions, canOpen/canViewReports helpers
  layout/AppLayout.tsx  App shell + navigation drawer
  pages/                One component per screen (Dashboard, Transactions, …)
  docs/                 In-app documentation content (Markdown)
  types.ts              Shared TypeScript types mirroring backend DTOs
```

## Data flow example — posting a transaction

1. The user submits the **Transactions** form in the frontend.
2. Axios `POST /api/transactions` with the JWT attached.
3. `TransactionsController` checks the `Transactions / Add` permission.
4. `TransactionService` validates and updates balances, writing a
   `TransactionRecord`, optional `BonusPointEntry`, and `LedgerEntry` rows in a
   single unit of work.
5. The updated balances are returned and shown to the user.
