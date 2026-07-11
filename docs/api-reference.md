# API Reference

The backend exposes a REST API under the `/api` prefix. Interactive documentation
is available via **Swagger** at `/swagger` when the API runs in the `Development`
environment.

- **Base URL (dev):** `http://localhost:5000/api`
- **Auth:** send `Authorization: Bearer <token>` on all endpoints except login.
- **Content type:** `application/json`

## Authentication

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/auth/login` | Exchange username/password for a JWT. Public. |
| `GET` | `/auth/permissions` | Permissions for the current user's role. |

Example login:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"Admin@12345"}'
```

Response:

```json
{ "token": "<jwt>", "userName": "admin", "fullName": "System Administrator", "role": "Admin" }
```

Use the returned token on subsequent calls:

```bash
curl http://localhost:5000/api/master-data/customers \
  -H "Authorization: Bearer <jwt>"
```

## Master data

Each resource supports listing (`GET`) and creation (`POST`). Access is guarded by
per-form permissions.

| Method | Endpoint | Permission |
| --- | --- | --- |
| `GET` / `POST` | `/master-data/users` | Users |
| `GET` / `POST` | `/master-data/locations` | Locations |
| `GET` / `POST` | `/master-data/cashiers` | Cashiers |
| `GET` / `POST` | `/master-data/customers` | Customers |
| `GET` / `POST` | `/master-data/members` | Members |
| `GET` / `POST` | `/master-data/games` | Games |
| `GET` | `/master-data/accounts` | Reports (view) |
| `GET` | `/master-data/game-modes` | Transactions |

## Transactions

| Method | Endpoint | Permission |
| --- | --- | --- |
| `POST` | `/transactions` | Transactions (add) |

Request body:

```json
{
  "customerId": "<guid>",
  "cashierId": "<guid>",
  "locationId": "<guid>",
  "gameModeId": "<guid>",
  "transactionType": "AddMoney",
  "shift": "Day",
  "amount": 100,
  "bonusPoints": 10,
  "notes": "Optional note"
}
```

`transactionType` is one of `AddMoney`, `WithdrawMoney`, `BonusPoints`;
`shift` is `Day` or `Night`.

## Expenses

| Method | Endpoint | Permission |
| --- | --- | --- |
| `GET` | `/expenses` | Expenses (open) |
| `POST` | `/expenses` | Expenses (add) |

## Reports

All report endpoints accept optional query filters: `from`, `to`, `locationId`,
`cashierId`, `gameModeId`.

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/reports/dashboard` | Dashboard KPIs |
| `GET` | `/reports/cashier-cash-register` | Cash in/out/net by cashier |
| `GET` | `/reports/bonus-points-summary` | Bonus points issued per customer |
| `GET` | `/reports/general-ledger` | Ledger entries |
| `GET` | `/reports/income-statement` | Income statement |
| `GET` | `/reports/profit-loss` | Profit & loss (income statement) |
| `GET` | `/reports/balance-sheet` | Balance sheet |

## Error responses

| Status | Meaning |
| --- | --- |
| `400 Bad Request` | Validation or business-rule failure (`{ "error": "..." }`) |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Authenticated but lacking the required permission |
