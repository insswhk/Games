# User Guide

This guide explains the roles and screens available in Game Center CRM.

## Roles

| Role | Typical access |
| --- | --- |
| **Admin** | Full access to every screen, including Users and Documentation. |
| **Manager** | Most screens for their location; cannot manage Users. |
| **Cashier** | Dashboard, Customers, Transactions, and Bonus Points. |

Navigation items are shown only for screens your role is allowed to open. The
backend enforces the same rules, so hidden features are also blocked at the API.

## Signing in

1. Open the app (default <http://localhost:5173>).
2. Enter your username and password and submit.
3. You land on the **Dashboard**. Use **Sign out** at the bottom of the sidebar to
   end your session.

## Screens

### Dashboard
Live KPIs: total cash in/out, net profit today, bonus points issued, active
customers, and active games.

### Users *(Admin)*
Operator accounts with their role and location scope.

### Locations
Game club records: club name, address, contacts, manager, and caretaker.

### Cashiers
Cashiers assigned to a location, including their cash register balance.

### Customers
Customer profiles with balances, referrals, and bonus points.

### Members
Membership numbers, types, and expiry dates.

### Transactions
Post **Add Money**, **Withdraw Money**, and **Bonus Point** entries:

1. Select the location, cashier, customer, and game mode.
2. Choose the transaction type and shift (Day/Night).
3. Enter the amount and/or bonus points and optional notes.
4. Click **Post Transaction**. A confirmation shows the new customer balance and
   cashier register total. Validation errors (e.g. insufficient register funds)
   are shown in a red banner.

### Bonus Points
A report of bonus points issued per customer.

### Expenses
Record and review operational expenses (rent, salary, refreshments, etc.).

### Games Register
Game assets with supplier information and maintenance cost tracking.

### Reports
Filter financial reports by date range, location, cashier, and game mode:

- **Profit & Loss** and **Balance Sheet** summaries
- **Cashier Cash Register** report
- **General Ledger** entries

### Documentation *(Admin)*
The in-app documentation hub (this content), including the Quick Start, Setup,
Architecture, User Guide, and API Reference.

## Tips

- Balances and money values are formatted for readability but stored precisely.
- If a screen shows "Unable to load data", check that the API is running and that
  your role has permission to open that screen.
