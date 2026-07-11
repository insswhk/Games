using GameCenter.Domain;

namespace GameCenter.Application;

public sealed record LoginRequest(string UserName, string Password);
public sealed record AuthResponse(string Token, string UserName, string FullName, UserRole Role, Guid? LocationId);
public sealed record PermissionDto(string FormName, bool CanOpen, bool CanAdd, bool CanDelete, bool CanViewReports);

public sealed record UserDto(Guid Id, string UserName, string FullName, UserRole Role, Guid? LocationId, bool IsActive);
public sealed record CreateUserRequest(string UserName, string Password, string FullName, UserRole Role, Guid? LocationId);

public sealed record LocationDto(
    Guid Id,
    string ClubName,
    string Address,
    string City,
    string State,
    string Country,
    string Phone,
    string Mobile,
    string WhatsApp,
    string Email,
    string Manager,
    string Caretaker);

public sealed record CreateLocationRequest(
    string ClubName,
    string Address,
    string City,
    string State,
    string Country,
    string Phone,
    string Mobile,
    string WhatsApp,
    string Email,
    string Manager,
    string Caretaker);

public sealed record CashierDto(Guid Id, Guid LocationId, string LocationName, string CashierCode, string FullName, decimal CashRegisterBalance, bool IsActive);
public sealed record CreateCashierRequest(Guid LocationId, Guid? AppUserId, string CashierCode, string FullName);

public sealed record CustomerDto(
    Guid Id,
    Guid LocationId,
    string LocationName,
    string CustomerCode,
    string FullName,
    string Phone,
    string Mobile,
    string WhatsApp,
    string Email,
    string Address,
    Guid? ReferralCustomerId,
    decimal Balance,
    int BonusPoints,
    bool IsActive);

public sealed record CreateCustomerRequest(
    Guid LocationId,
    string CustomerCode,
    string FullName,
    string Phone,
    string Mobile,
    string WhatsApp,
    string Email,
    string Address,
    Guid? ReferralCustomerId);

public sealed record MemberDto(Guid Id, Guid LocationId, Guid? CustomerId, string MembershipNumber, string MembershipType, DateOnly ExpiryDate, bool IsActive);
public sealed record CreateMemberRequest(Guid LocationId, Guid? CustomerId, string MembershipNumber, string MembershipType, DateOnly ExpiryDate);

public sealed record GameModeDto(Guid Id, GameModeType ModeType, string Code, string Name);

public sealed record GameRegisterDto(
    Guid Id,
    Guid LocationId,
    string GameName,
    int NumberOfPlayers,
    decimal PurchaseAmount,
    string SupplierInfo,
    string MaintenanceContacts,
    decimal MaintenanceCosts,
    DateOnly? LastMaintenanceDate,
    bool IsActive);

public sealed record CreateGameRegisterRequest(
    Guid LocationId,
    string GameName,
    int NumberOfPlayers,
    decimal PurchaseAmount,
    string SupplierInfo,
    string MaintenanceContacts,
    decimal MaintenanceCosts,
    DateOnly? LastMaintenanceDate);

public sealed record AccountDto(Guid Id, string AccountNumber, string AccountName, AccountType AccountType);

public sealed record CreateTransactionRequest(
    Guid CustomerId,
    Guid CashierId,
    Guid LocationId,
    Guid GameModeId,
    TransactionType TransactionType,
    ShiftType Shift,
    decimal Amount,
    int BonusPoints,
    string Notes);

public sealed record TransactionResultDto(
    Guid TransactionId,
    decimal CustomerBalanceAfter,
    decimal CashierRegisterAfter,
    int CustomerBonusPointsAfter);

public sealed record CreateExpenseRequest(
    Guid AccountId,
    Guid LocationId,
    ExpenseType ExpenseType,
    decimal Amount,
    string Notes);

public sealed record ExpenseDto(Guid Id, Guid AccountId, Guid LocationId, ExpenseType ExpenseType, decimal Amount, string Notes, DateTimeOffset ExpenseDate);

public sealed record ReportFilter(DateTimeOffset? From, DateTimeOffset? To, Guid? LocationId, Guid? CashierId, Guid? GameModeId);

public sealed record DashboardKpiDto(
    decimal TotalCashIn,
    decimal TotalCashOut,
    decimal NetProfitToday,
    int BonusPointsIssued,
    int ActiveCustomers,
    int ActiveGames);

public sealed record CashierCashRegisterReportRow(
    Guid CashierId,
    string CashierCode,
    string CashierName,
    decimal TotalCashIn,
    decimal TotalCashOut,
    decimal NetCash);

public sealed record BonusPointsReportRow(Guid CustomerId, string CustomerCode, string CustomerName, int PointsIssued);

public sealed record LedgerReportRow(
    DateTimeOffset EntryDate,
    string AccountNumber,
    string AccountName,
    AccountType AccountType,
    string LocationName,
    decimal Debit,
    decimal Credit,
    string Description);

public sealed record IncomeStatementDto(decimal TotalIncome, decimal TotalExpenses, decimal NetProfit);
public sealed record BalanceSheetDto(decimal Assets, decimal Liabilities, decimal Equity);
