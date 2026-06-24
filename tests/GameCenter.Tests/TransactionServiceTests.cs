using GameCenter.Application;
using GameCenter.Domain;
using GameCenter.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GameCenter.Tests;

public sealed class TransactionServiceTests
{
    [Fact]
    public async Task AddMoney_updates_customer_cashier_ledger_and_bonus_points()
    {
        await using var db = CreateDbContext();
        var data = await SeedMinimalDataAsync(db);
        var service = CreateService(db);

        var result = await service.CreateAsync(new CreateTransactionRequest(
            data.CustomerId,
            data.CashierId,
            data.LocationId,
            data.GameModeId,
            TransactionType.AddMoney,
            ShiftType.Day,
            100m,
            10,
            "Unit test add"));

        var customer = await db.Customers.SingleAsync(x => x.Id == data.CustomerId);
        var cashier = await db.Cashiers.SingleAsync(x => x.Id == data.CashierId);

        Assert.Equal(150m, customer.Balance);
        Assert.Equal(600m, cashier.CashRegisterBalance);
        Assert.Equal(15, customer.BonusPoints);
        Assert.Equal(customer.Balance, result.CustomerBalanceAfter);
        Assert.Equal(4, await db.LedgerEntries.CountAsync());
        Assert.Single(db.BonusPointEntries);
    }

    [Fact]
    public async Task WithdrawMoney_prevents_customer_overdraft()
    {
        await using var db = CreateDbContext();
        var data = await SeedMinimalDataAsync(db);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateTransactionRequest(
                data.CustomerId,
                data.CashierId,
                data.LocationId,
                data.GameModeId,
                TransactionType.WithdrawMoney,
                ShiftType.Night,
                75m,
                0,
                "Unit test withdrawal")));

        Assert.Contains("overdrawn", exception.Message);
        Assert.Equal(50m, (await db.Customers.SingleAsync(x => x.Id == data.CustomerId)).Balance);
        Assert.Empty(db.Transactions);
    }

    private static GameCenterDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new GameCenterDbContext(options, new TestCurrentUser());
    }

    private static ITransactionService CreateService(GameCenterDbContext db)
    {
        return new TransactionService(
            new EfRepository<Customer>(db),
            new EfRepository<Cashier>(db),
            new EfRepository<Location>(db),
            new EfRepository<GameMode>(db),
            new EfRepository<TransactionRecord>(db),
            new EfRepository<BonusPointEntry>(db),
            new EfRepository<Account>(db),
            new EfRepository<LedgerEntry>(db),
            new EfUnitOfWork(db),
            new TestCurrentUser());
    }

    private static async Task<TestIds> SeedMinimalDataAsync(GameCenterDbContext db)
    {
        var location = new Location { ClubName = "Test Club" };
        var cashier = new Cashier { LocationId = location.Id, CashierCode = "T-CASH", FullName = "Test Cashier", CashRegisterBalance = 500m };
        var customer = new Customer { LocationId = location.Id, CustomerCode = "T-CUS", FullName = "Test Customer", Balance = 50m, BonusPoints = 5 };
        var mode = new GameMode { Code = "A", Name = "Club Games", ModeType = GameModeType.ClubGames };
        var accounts = new[]
        {
            new Account { AccountNumber = "1000", AccountName = "Cash On Hand", AccountType = AccountType.Asset },
            new Account { AccountNumber = "2100", AccountName = "Bonus Liability", AccountType = AccountType.Liability },
            new Account { AccountNumber = "4000", AccountName = "Game Revenue", AccountType = AccountType.Income },
            new Account { AccountNumber = "5000", AccountName = "Player Payouts", AccountType = AccountType.Expense },
            new Account { AccountNumber = "5100", AccountName = "Bonus Expense", AccountType = AccountType.Expense }
        };

        db.Add(location);
        db.Add(cashier);
        db.Add(customer);
        db.Add(mode);
        db.AddRange(accounts);
        await db.SaveChangesAsync();

        return new TestIds(location.Id, cashier.Id, customer.Id, mode.Id);
    }

    private sealed record TestIds(Guid LocationId, Guid CashierId, Guid CustomerId, Guid GameModeId);

    private sealed class TestCurrentUser : ICurrentUserService
    {
        public string UserName => "unit-test";
        public UserRole? Role => UserRole.Admin;
        public Guid? LocationId => null;
    }
}
