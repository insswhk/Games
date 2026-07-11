using GameCenter.Domain;

namespace GameCenter.Application;

public interface IRepository<TEntity> where TEntity : Entity
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IAppTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    string UserName { get; }
    UserRole? Role { get; }
    Guid? LocationId { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenFactory
{
    string CreateToken(AppUser user);
}

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task EnsureAsync(string formName, PermissionAction action, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(UserRole role, CancellationToken cancellationToken = default);
}

public interface ITransactionService
{
    Task<TransactionResultDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
}

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAsync(CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);
}

public interface IReportingService
{
    Task<DashboardKpiDto> GetDashboardAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashierCashRegisterReportRow>> GetCashierCashRegisterAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BonusPointsReportRow>> GetBonusPointsSummaryAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LedgerReportRow>> GetGeneralLedgerAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<IncomeStatementDto> GetIncomeStatementAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<BalanceSheetDto> GetBalanceSheetAsync(ReportFilter filter, CancellationToken cancellationToken = default);
}

public interface IMasterDataService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default);
    Task<LocationDto> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashierDto>> GetCashiersAsync(CancellationToken cancellationToken = default);
    Task<CashierDto> CreateCashierAsync(CreateCashierRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default);
    Task<MemberDto> CreateMemberAsync(CreateMemberRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameRegisterDto>> GetGamesAsync(CancellationToken cancellationToken = default);
    Task<GameRegisterDto> CreateGameAsync(CreateGameRegisterRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameModeDto>> GetGameModesAsync(CancellationToken cancellationToken = default);
}

public enum PermissionAction
{
    Open,
    Add,
    Delete,
    ViewReports
}
