namespace GameCenter.Domain;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Cashier = 3
}

public enum GameModeType
{
    ClubGames = 1,
    JokerGames = 2
}

public enum TransactionType
{
    AddMoney = 1,
    WithdrawMoney = 2,
    BonusPoints = 3
}

public enum ShiftType
{
    Day = 1,
    Night = 2
}

public enum AccountType
{
    Income = 1,
    Expense = 2,
    Asset = 3,
    Liability = 4
}

public enum ExpenseType
{
    Startup = 1,
    Rent = 2,
    Salary = 3,
    Furniture = 4,
    Games = 5,
    Refreshments = 6
}

public sealed class AppUser : Entity
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? LocationId { get; set; }
    public Location? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Permission : Entity
{
    public UserRole Role { get; set; }
    public string FormName { get; set; } = string.Empty;
    public bool CanOpen { get; set; }
    public bool CanAdd { get; set; }
    public bool CanDelete { get; set; }
    public bool CanViewReports { get; set; }
}

public sealed class Location : Entity
{
    public string ClubName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Caretaker { get; set; } = string.Empty;
}

public sealed class Cashier : Entity
{
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public string CashierCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal CashRegisterBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class Customer : Entity
{
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid? ReferralCustomerId { get; set; }
    public Customer? ReferralCustomer { get; set; }
    public decimal Balance { get; set; }
    public int BonusPoints { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class Member : Entity
{
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public string MembershipType { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class GameMode : Entity
{
    public GameModeType ModeType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class TransactionRecord : Entity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public Guid CashierId { get; set; }
    public Cashier Cashier { get; set; } = default!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid GameModeId { get; set; }
    public GameMode GameMode { get; set; } = default!;
    public TransactionType TransactionType { get; set; }
    public ShiftType Shift { get; set; }
    public decimal Amount { get; set; }
    public int BonusPoints { get; set; }
    public decimal CustomerBalanceAfter { get; set; }
    public decimal CashierRegisterAfter { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class BonusPointEntry : Entity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public Guid? TransactionRecordId { get; set; }
    public TransactionRecord? TransactionRecord { get; set; }
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid CashierId { get; set; }
    public Cashier Cashier { get; set; } = default!;
    public int Points { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string Notes { get; set; } = string.Empty;
}

public sealed class Account : Entity
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
}

public sealed class LedgerEntry : Entity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid? TransactionRecordId { get; set; }
    public TransactionRecord? TransactionRecord { get; set; }
    public Guid? ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset EntryDate { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class Expense : Entity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public ExpenseType ExpenseType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset ExpenseDate { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class GameRegister : Entity
{
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
    public string GameName { get; set; } = string.Empty;
    public int NumberOfPlayers { get; set; }
    public decimal PurchaseAmount { get; set; }
    public string SupplierInfo { get; set; } = string.Empty;
    public string MaintenanceContacts { get; set; } = string.Empty;
    public decimal MaintenanceCosts { get; set; }
    public DateOnly? LastMaintenanceDate { get; set; }
    public bool IsActive { get; set; } = true;
}
