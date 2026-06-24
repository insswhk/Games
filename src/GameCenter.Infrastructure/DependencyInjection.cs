using GameCenter.Application;
using GameCenter.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GameCenterDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IMasterDataService, MasterDataService>();

        return services;
    }

    public static async Task SeedGameCenterAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GameCenterDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var now = DateTimeOffset.UtcNow;

        var locations = new[]
        {
            new Location
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                ClubName = "Downtown Game Club",
                Address = "100 Main Street",
                City = "Austin",
                State = "Texas",
                Country = "USA",
                Phone = "512-555-0100",
                Mobile = "512-555-0101",
                WhatsApp = "512-555-0102",
                Email = "downtown@example.com",
                Manager = "Maria Garcia",
                Caretaker = "Leo Stone"
            },
            new Location
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                ClubName = "Northside Joker Lounge",
                Address = "250 North Avenue",
                City = "Dallas",
                State = "Texas",
                Country = "USA",
                Phone = "214-555-0110",
                Mobile = "214-555-0111",
                WhatsApp = "214-555-0112",
                Email = "northside@example.com",
                Manager = "Sam Taylor",
                Caretaker = "Nina Park"
            }
        };

        var users = new[]
        {
            new AppUser { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), UserName = "admin", FullName = "System Administrator", Role = UserRole.Admin, PasswordHash = passwordHasher.Hash("Admin@12345") },
            new AppUser { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), UserName = "manager", FullName = "Club Manager", Role = UserRole.Manager, LocationId = locations[0].Id, PasswordHash = passwordHasher.Hash("Manager@12345") },
            new AppUser { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), UserName = "cashier1", FullName = "Day Cashier", Role = UserRole.Cashier, LocationId = locations[0].Id, PasswordHash = passwordHasher.Hash("Cashier@12345") },
            new AppUser { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), UserName = "cashier2", FullName = "Night Cashier", Role = UserRole.Cashier, LocationId = locations[1].Id, PasswordHash = passwordHasher.Hash("Cashier@12345") }
        };

        var forms = new[] { "Dashboard", "Users", "Locations", "Cashiers", "Customers", "Members", "Transactions", "BonusPoints", "Expenses", "Games", "Reports" };
        var permissions = forms.SelectMany(form => new[]
        {
            new Permission { Role = UserRole.Admin, FormName = form, CanOpen = true, CanAdd = true, CanDelete = true, CanViewReports = true },
            new Permission { Role = UserRole.Manager, FormName = form, CanOpen = form != "Users", CanAdd = form != "Users", CanDelete = false, CanViewReports = form is "Dashboard" or "Reports" or "BonusPoints" },
            new Permission { Role = UserRole.Cashier, FormName = form, CanOpen = form is "Dashboard" or "Customers" or "Transactions" or "BonusPoints", CanAdd = form is "Customers" or "Transactions", CanDelete = false, CanViewReports = form is "Dashboard" or "BonusPoints" }
        }).ToList();

        var cashiers = new[]
        {
            new Cashier { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), LocationId = locations[0].Id, AppUserId = users[2].Id, CashierCode = "CASH-A-001", FullName = users[2].FullName, CashRegisterBalance = 1_000m },
            new Cashier { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), LocationId = locations[1].Id, AppUserId = users[3].Id, CashierCode = "CASH-B-001", FullName = users[3].FullName, CashRegisterBalance = 1_000m }
        };

        var gameModes = new[]
        {
            new GameMode { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), ModeType = GameModeType.ClubGames, Code = "A", Name = "Club Games" },
            new GameMode { Id = Guid.Parse("40000000-0000-0000-0000-000000000002"), ModeType = GameModeType.JokerGames, Code = "B", Name = "Joker Games" }
        };

        var accounts = new[]
        {
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), AccountNumber = "1000", AccountName = "Cash On Hand", AccountType = AccountType.Asset },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000002"), AccountNumber = "2000", AccountName = "Customer Balance Liability", AccountType = AccountType.Liability },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000003"), AccountNumber = "2100", AccountName = "Bonus Points Liability", AccountType = AccountType.Liability },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000004"), AccountNumber = "4000", AccountName = "Game Revenue", AccountType = AccountType.Income },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000005"), AccountNumber = "5000", AccountName = "Player Payouts", AccountType = AccountType.Expense },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000006"), AccountNumber = "5100", AccountName = "Promotional Bonus Expense", AccountType = AccountType.Expense },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000007"), AccountNumber = "5200", AccountName = "Rent Expense", AccountType = AccountType.Expense },
            new Account { Id = Guid.Parse("50000000-0000-0000-0000-000000000008"), AccountNumber = "5300", AccountName = "Salary Expense", AccountType = AccountType.Expense }
        };

        var customers = Enumerable.Range(1, 10).Select(i =>
        {
            var location = i <= 5 ? locations[0] : locations[1];
            return new Customer
            {
                Id = Guid.Parse($"60000000-0000-0000-0000-{i:000000000000}"),
                LocationId = location.Id,
                CustomerCode = $"CUS-{i:000}",
                FullName = $"Customer {i:00}",
                Phone = $"555-10{i:00}",
                Mobile = $"555-20{i:00}",
                WhatsApp = $"555-30{i:00}",
                Email = $"customer{i}@example.com",
                Address = $"{i} Player Lane",
                Balance = i * 25,
                BonusPoints = i * 3
            };
        }).ToArray();

        customers[1].ReferralCustomerId = customers[0].Id;
        customers[2].ReferralCustomerId = customers[0].Id;

        var members = customers.Take(4).Select((customer, index) => new Member
        {
            LocationId = customer.LocationId,
            CustomerId = customer.Id,
            MembershipNumber = $"MEM-{index + 1:000}",
            MembershipType = index % 2 == 0 ? "Gold" : "Silver",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12 + index))
        }).ToArray();

        var games = new[]
        {
            new GameRegister { LocationId = locations[0].Id, GameName = "Velocity Racing", NumberOfPlayers = 4, PurchaseAmount = 12_000m, SupplierInfo = "Arcade Pro Supply", MaintenanceContacts = "support@arcadepro.example", MaintenanceCosts = 500m, LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)) },
            new GameRegister { LocationId = locations[0].Id, GameName = "Space Duel", NumberOfPlayers = 2, PurchaseAmount = 9_500m, SupplierInfo = "Galaxy Games", MaintenanceContacts = "555-444-1000", MaintenanceCosts = 300m, LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)) },
            new GameRegister { LocationId = locations[1].Id, GameName = "Joker Wheel", NumberOfPlayers = 6, PurchaseAmount = 15_000m, SupplierInfo = "Joker Systems", MaintenanceContacts = "service@joker.example", MaintenanceCosts = 800m, LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)) },
            new GameRegister { LocationId = locations[1].Id, GameName = "Mega Cards", NumberOfPlayers = 5, PurchaseAmount = 11_000m, SupplierInfo = "TableTech", MaintenanceContacts = "555-444-2000", MaintenanceCosts = 350m, LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)) },
            new GameRegister { LocationId = locations[0].Id, GameName = "Club Slots", NumberOfPlayers = 1, PurchaseAmount = 7_000m, SupplierInfo = "SlotWorks", MaintenanceContacts = "repair@slotworks.example", MaintenanceCosts = 200m, LastMaintenanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)) }
        };

        db.AddRange(locations);
        db.AddRange(users);
        db.AddRange(permissions);
        db.AddRange(cashiers);
        db.AddRange(gameModes);
        db.AddRange(accounts);
        db.AddRange(customers);
        db.AddRange(members);
        db.AddRange(games);

        var transactions = new[]
        {
            CreateTransaction(customers[0], cashiers[0], locations[0], gameModes[0], TransactionType.AddMoney, ShiftType.Day, 100m, 10, now.AddHours(-3)),
            CreateTransaction(customers[1], cashiers[0], locations[0], gameModes[1], TransactionType.WithdrawMoney, ShiftType.Day, 25m, 0, now.AddHours(-2)),
            CreateTransaction(customers[5], cashiers[1], locations[1], gameModes[1], TransactionType.AddMoney, ShiftType.Night, 150m, 15, now.AddHours(-1)),
            CreateTransaction(customers[6], cashiers[1], locations[1], gameModes[0], TransactionType.BonusPoints, ShiftType.Night, 0m, 20, now.AddMinutes(-30))
        };

        db.AddRange(transactions);
        db.AddRange(transactions.Where(x => x.BonusPoints > 0).Select(x => new BonusPointEntry
        {
            CustomerId = x.CustomerId,
            CashierId = x.CashierId,
            LocationId = x.LocationId,
            TransactionRecordId = x.Id,
            Points = x.BonusPoints,
            OccurredAt = x.OccurredAt,
            Notes = "Seeded bonus points"
        }));

        db.AddRange(CreateSeedLedgers(transactions, accounts));
        await db.SaveChangesAsync();
    }

    private static TransactionRecord CreateTransaction(Customer customer, Cashier cashier, Location location, GameMode mode, TransactionType type, ShiftType shift, decimal amount, int points, DateTimeOffset occurredAt)
    {
        return new TransactionRecord
        {
            CustomerId = customer.Id,
            CashierId = cashier.Id,
            LocationId = location.Id,
            GameModeId = mode.Id,
            TransactionType = type,
            Shift = shift,
            Amount = amount,
            BonusPoints = points,
            CustomerBalanceAfter = customer.Balance,
            CashierRegisterAfter = cashier.CashRegisterBalance,
            Notes = "Seed transaction",
            OccurredAt = occurredAt
        };
    }

    private static IEnumerable<LedgerEntry> CreateSeedLedgers(IEnumerable<TransactionRecord> transactions, Account[] accounts)
    {
        Account account(string number) => accounts.Single(x => x.AccountNumber == number);

        foreach (var transaction in transactions)
        {
            if (transaction.TransactionType == TransactionType.AddMoney)
            {
                yield return Ledger(account("1000"), transaction, transaction.Amount, 0, "Seed cash received");
                yield return Ledger(account("4000"), transaction, 0, transaction.Amount, "Seed game revenue");
            }
            else if (transaction.TransactionType == TransactionType.WithdrawMoney)
            {
                yield return Ledger(account("5000"), transaction, transaction.Amount, 0, "Seed player payout");
                yield return Ledger(account("1000"), transaction, 0, transaction.Amount, "Seed cash paid");
            }

            if (transaction.BonusPoints > 0)
            {
                yield return Ledger(account("5100"), transaction, transaction.BonusPoints, 0, "Seed bonus expense");
                yield return Ledger(account("2100"), transaction, 0, transaction.BonusPoints, "Seed bonus liability");
            }
        }
    }

    private static LedgerEntry Ledger(Account account, TransactionRecord transaction, decimal debit, decimal credit, string description) =>
        new()
        {
            AccountId = account.Id,
            LocationId = transaction.LocationId,
            TransactionRecordId = transaction.Id,
            Debit = debit,
            Credit = credit,
            Description = description,
            EntryDate = transaction.OccurredAt
        };
}
