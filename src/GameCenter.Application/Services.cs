using GameCenter.Domain;

namespace GameCenter.Application;

public sealed class AuthService(
    IRepository<AppUser> users,
    IPasswordHasher passwordHasher,
    IJwtTokenFactory jwtTokenFactory) : IAuthService
{
    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = users.Query()
            .FirstOrDefault(x => !x.IsDeleted && x.IsActive && x.UserName == request.UserName);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid user name or password.");
        }

        var token = jwtTokenFactory.CreateToken(user);
        return Task.FromResult(new AuthResponse(token, user.UserName, user.FullName, user.Role, user.LocationId));
    }
}

public sealed class PermissionService(
    IRepository<Permission> permissions,
    ICurrentUserService currentUser) : IPermissionService
{
    public Task EnsureAsync(string formName, PermissionAction action, CancellationToken cancellationToken = default)
    {
        if (currentUser.Role is null)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        var permission = permissions.Query()
            .FirstOrDefault(x => !x.IsDeleted && x.Role == currentUser.Role.Value && x.FormName == formName);

        var allowed = action switch
        {
            PermissionAction.Open => permission?.CanOpen == true,
            PermissionAction.Add => permission?.CanAdd == true,
            PermissionAction.Delete => permission?.CanDelete == true,
            PermissionAction.ViewReports => permission?.CanViewReports == true,
            _ => false
        };

        if (!allowed)
        {
            throw new UnauthorizedAccessException($"Role {currentUser.Role} cannot {action} {formName}.");
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var rows = permissions.Query()
            .Where(x => !x.IsDeleted && x.Role == role)
            .OrderBy(x => x.FormName)
            .Select(x => new PermissionDto(x.FormName, x.CanOpen, x.CanAdd, x.CanDelete, x.CanViewReports))
            .ToList();

        return Task.FromResult<IReadOnlyList<PermissionDto>>(rows);
    }
}

public sealed class TransactionService(
    IRepository<Customer> customers,
    IRepository<Cashier> cashiers,
    IRepository<Location> locations,
    IRepository<GameMode> gameModes,
    IRepository<TransactionRecord> transactions,
    IRepository<BonusPointEntry> bonusPoints,
    IRepository<Account> accounts,
    IRepository<LedgerEntry> ledgerEntries,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ITransactionService
{
    private const string CashOnHandAccount = "1000";
    private const string GameRevenueAccount = "4000";
    private const string PlayerPayoutExpenseAccount = "5000";
    private const string BonusExpenseAccount = "5100";
    private const string BonusLiabilityAccount = "2100";

    public async Task<TransactionResultDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await using var dbTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = customers.Query().FirstOrDefault(x => !x.IsDeleted && x.Id == request.CustomerId && x.IsActive)
                ?? throw new InvalidOperationException("Customer was not found or is inactive.");
            var cashier = cashiers.Query().FirstOrDefault(x => !x.IsDeleted && x.Id == request.CashierId && x.IsActive)
                ?? throw new InvalidOperationException("Cashier was not found or is inactive.");
            _ = locations.Query().FirstOrDefault(x => !x.IsDeleted && x.Id == request.LocationId)
                ?? throw new InvalidOperationException("Location was not found.");
            _ = gameModes.Query().FirstOrDefault(x => !x.IsDeleted && x.Id == request.GameModeId)
                ?? throw new InvalidOperationException("Game mode was not found.");

            if (customer.LocationId != request.LocationId || cashier.LocationId != request.LocationId)
            {
                throw new InvalidOperationException("Customer and cashier must belong to the transaction location.");
            }

            if (currentUser.Role == UserRole.Cashier && currentUser.LocationId is not null && currentUser.LocationId != request.LocationId)
            {
                throw new UnauthorizedAccessException("Cashiers can only post transactions for their assigned location.");
            }

            ApplyBalances(request, customer, cashier);

            var transaction = new TransactionRecord
            {
                CustomerId = request.CustomerId,
                CashierId = request.CashierId,
                LocationId = request.LocationId,
                GameModeId = request.GameModeId,
                TransactionType = request.TransactionType,
                Shift = request.Shift,
                Amount = request.Amount,
                BonusPoints = request.BonusPoints,
                CustomerBalanceAfter = customer.Balance,
                CashierRegisterAfter = cashier.CashRegisterBalance,
                Notes = request.Notes,
                CreatedBy = currentUser.UserName
            };

            await transactions.AddAsync(transaction, cancellationToken);
            await AddLedgerEntriesAsync(request, transaction, cancellationToken);

            if (request.BonusPoints > 0)
            {
                await bonusPoints.AddAsync(new BonusPointEntry
                {
                    CustomerId = customer.Id,
                    TransactionRecordId = transaction.Id,
                    CashierId = cashier.Id,
                    LocationId = request.LocationId,
                    Points = request.BonusPoints,
                    Notes = request.Notes,
                    CreatedBy = currentUser.UserName
                }, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return new TransactionResultDto(transaction.Id, customer.Balance, cashier.CashRegisterBalance, customer.BonusPoints);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void ValidateRequest(CreateTransactionRequest request)
    {
        if (request.CustomerId == Guid.Empty || request.CashierId == Guid.Empty || request.LocationId == Guid.Empty || request.GameModeId == Guid.Empty)
        {
            throw new InvalidOperationException("Customer, cashier, location, and game mode are required.");
        }

        if (request.TransactionType is TransactionType.AddMoney or TransactionType.WithdrawMoney && request.Amount <= 0)
        {
            throw new InvalidOperationException("Add and withdrawal transactions require a positive amount.");
        }

        if (request.TransactionType == TransactionType.BonusPoints && request.BonusPoints <= 0)
        {
            throw new InvalidOperationException("Bonus point transactions require positive points.");
        }

        if (request.Amount < 0 || request.BonusPoints < 0)
        {
            throw new InvalidOperationException("Amount and bonus points cannot be negative.");
        }
    }

    private static void ApplyBalances(CreateTransactionRequest request, Customer customer, Cashier cashier)
    {
        switch (request.TransactionType)
        {
            case TransactionType.AddMoney:
                customer.Balance += request.Amount;
                cashier.CashRegisterBalance += request.Amount;
                break;
            case TransactionType.WithdrawMoney:
                if (customer.Balance < request.Amount)
                {
                    throw new InvalidOperationException("Withdrawal denied: customer balance would be overdrawn.");
                }
                if (cashier.CashRegisterBalance < request.Amount)
                {
                    throw new InvalidOperationException("Withdrawal denied: cashier register would be overdrawn.");
                }
                customer.Balance -= request.Amount;
                cashier.CashRegisterBalance -= request.Amount;
                break;
            case TransactionType.BonusPoints:
                break;
            default:
                throw new InvalidOperationException("Unsupported transaction type.");
        }

        customer.BonusPoints += request.BonusPoints;
    }

    private async Task AddLedgerEntriesAsync(CreateTransactionRequest request, TransactionRecord transaction, CancellationToken cancellationToken)
    {
        var cash = RequireAccount(CashOnHandAccount);
        var gameRevenue = RequireAccount(GameRevenueAccount);
        var playerPayouts = RequireAccount(PlayerPayoutExpenseAccount);
        var bonusExpense = RequireAccount(BonusExpenseAccount);
        var bonusLiability = RequireAccount(BonusLiabilityAccount);

        if (request.TransactionType == TransactionType.AddMoney)
        {
            await AddLedgerAsync(cash.Id, request.LocationId, transaction.Id, request.Amount, 0, "Cash received from customer", cancellationToken);
            await AddLedgerAsync(gameRevenue.Id, request.LocationId, transaction.Id, 0, request.Amount, "Game revenue from customer add money", cancellationToken);
        }
        else if (request.TransactionType == TransactionType.WithdrawMoney)
        {
            await AddLedgerAsync(playerPayouts.Id, request.LocationId, transaction.Id, request.Amount, 0, "Player payout expense", cancellationToken);
            await AddLedgerAsync(cash.Id, request.LocationId, transaction.Id, 0, request.Amount, "Cash paid to customer", cancellationToken);
        }

        if (request.BonusPoints > 0)
        {
            await AddLedgerAsync(bonusExpense.Id, request.LocationId, transaction.Id, request.BonusPoints, 0, "Promotional bonus points issued", cancellationToken);
            await AddLedgerAsync(bonusLiability.Id, request.LocationId, transaction.Id, 0, request.BonusPoints, "Outstanding bonus point liability", cancellationToken);
        }
    }

    private Account RequireAccount(string accountNumber)
    {
        return accounts.Query().FirstOrDefault(x => !x.IsDeleted && x.AccountNumber == accountNumber)
            ?? throw new InvalidOperationException($"Required account {accountNumber} is missing.");
    }

    private Task AddLedgerAsync(Guid accountId, Guid locationId, Guid transactionId, decimal debit, decimal credit, string description, CancellationToken cancellationToken)
    {
        return ledgerEntries.AddAsync(new LedgerEntry
        {
            AccountId = accountId,
            LocationId = locationId,
            TransactionRecordId = transactionId,
            Debit = debit,
            Credit = credit,
            Description = description,
            CreatedBy = currentUser.UserName
        }, cancellationToken);
    }
}

public sealed class ExpenseService(
    IRepository<Expense> expenses,
    IRepository<Account> accounts,
    IRepository<LedgerEntry> ledgerEntries,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IExpenseService
{
    public Task<IReadOnlyList<ExpenseDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var rows = expenses.Query()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ExpenseDate)
            .Select(x => new ExpenseDto(x.Id, x.AccountId, x.LocationId, x.ExpenseType, x.Amount, x.Notes, x.ExpenseDate))
            .ToList();

        return Task.FromResult<IReadOnlyList<ExpenseDto>>(rows);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Expense amount must be positive.");
        }

        var expenseAccount = accounts.Query().FirstOrDefault(x => !x.IsDeleted && x.Id == request.AccountId)
            ?? throw new InvalidOperationException("Expense account was not found.");
        var cashAccount = accounts.Query().FirstOrDefault(x => !x.IsDeleted && x.AccountNumber == "1000")
            ?? throw new InvalidOperationException("Cash account is missing.");

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var expense = new Expense
        {
            AccountId = expenseAccount.Id,
            LocationId = request.LocationId,
            ExpenseType = request.ExpenseType,
            Amount = request.Amount,
            Debit = request.Amount,
            Credit = 0,
            Notes = request.Notes,
            CreatedBy = currentUser.UserName
        };

        await expenses.AddAsync(expense, cancellationToken);
        await ledgerEntries.AddAsync(new LedgerEntry
        {
            AccountId = expenseAccount.Id,
            LocationId = request.LocationId,
            ExpenseId = expense.Id,
            Debit = request.Amount,
            Credit = 0,
            Description = request.Notes,
            CreatedBy = currentUser.UserName
        }, cancellationToken);
        await ledgerEntries.AddAsync(new LedgerEntry
        {
            AccountId = cashAccount.Id,
            LocationId = request.LocationId,
            ExpenseId = expense.Id,
            Debit = 0,
            Credit = request.Amount,
            Description = $"Cash paid for {request.ExpenseType}",
            CreatedBy = currentUser.UserName
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new ExpenseDto(expense.Id, expense.AccountId, expense.LocationId, expense.ExpenseType, expense.Amount, expense.Notes, expense.ExpenseDate);
    }
}

public sealed class ReportingService(
    IRepository<TransactionRecord> transactions,
    IRepository<BonusPointEntry> bonusEntries,
    IRepository<LedgerEntry> ledgerEntries,
    IRepository<Customer> customers,
    IRepository<GameRegister> games) : IReportingService
{
    public Task<DashboardKpiDto> GetDashboardAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var todayFilter = filter with { From = filter.From ?? today, To = filter.To ?? today.AddDays(1).AddTicks(-1) };
        var scopedTransactions = FilterTransactions(todayFilter);
        var cashIn = scopedTransactions.Where(x => x.TransactionType == TransactionType.AddMoney).Sum(x => x.Amount);
        var cashOut = scopedTransactions.Where(x => x.TransactionType == TransactionType.WithdrawMoney).Sum(x => x.Amount);
        var bonus = scopedTransactions.Sum(x => x.BonusPoints);
        var activeCustomers = customers.Query().Count(x => !x.IsDeleted && x.IsActive && (filter.LocationId == null || x.LocationId == filter.LocationId));
        var activeGames = games.Query().Count(x => !x.IsDeleted && x.IsActive && (filter.LocationId == null || x.LocationId == filter.LocationId));

        return Task.FromResult(new DashboardKpiDto(cashIn, cashOut, cashIn - cashOut, bonus, activeCustomers, activeGames));
    }

    public Task<IReadOnlyList<CashierCashRegisterReportRow>> GetCashierCashRegisterAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var rows = FilterTransactions(filter)
            .GroupBy(x => new { x.CashierId, x.Cashier.CashierCode, x.Cashier.FullName })
            .Select(g => new CashierCashRegisterReportRow(
                g.Key.CashierId,
                g.Key.CashierCode,
                g.Key.FullName,
                g.Where(x => x.TransactionType == TransactionType.AddMoney).Sum(x => x.Amount),
                g.Where(x => x.TransactionType == TransactionType.WithdrawMoney).Sum(x => x.Amount),
                g.Where(x => x.TransactionType == TransactionType.AddMoney).Sum(x => x.Amount) - g.Where(x => x.TransactionType == TransactionType.WithdrawMoney).Sum(x => x.Amount)))
            .OrderBy(x => x.CashierCode)
            .ToList();

        return Task.FromResult<IReadOnlyList<CashierCashRegisterReportRow>>(rows);
    }

    public Task<IReadOnlyList<BonusPointsReportRow>> GetBonusPointsSummaryAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var query = bonusEntries.Query().Where(x => !x.IsDeleted);
        if (filter.From is not null) query = query.Where(x => x.OccurredAt >= filter.From);
        if (filter.To is not null) query = query.Where(x => x.OccurredAt <= filter.To);
        if (filter.LocationId is not null) query = query.Where(x => x.LocationId == filter.LocationId);
        if (filter.CashierId is not null) query = query.Where(x => x.CashierId == filter.CashierId);

        var rows = query
            .GroupBy(x => new { x.CustomerId, x.Customer.CustomerCode, x.Customer.FullName })
            .Select(g => new BonusPointsReportRow(g.Key.CustomerId, g.Key.CustomerCode, g.Key.FullName, g.Sum(x => x.Points)))
            .OrderByDescending(x => x.PointsIssued)
            .ToList();

        return Task.FromResult<IReadOnlyList<BonusPointsReportRow>>(rows);
    }

    public Task<IReadOnlyList<LedgerReportRow>> GetGeneralLedgerAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var query = FilterLedger(filter);
        var rows = query
            .OrderBy(x => x.EntryDate)
            .ThenBy(x => x.Account.AccountNumber)
            .Select(x => new LedgerReportRow(
                x.EntryDate,
                x.Account.AccountNumber,
                x.Account.AccountName,
                x.Account.AccountType,
                x.Location.ClubName,
                x.Debit,
                x.Credit,
                x.Description))
            .ToList();

        return Task.FromResult<IReadOnlyList<LedgerReportRow>>(rows);
    }

    public Task<IncomeStatementDto> GetIncomeStatementAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var query = FilterLedger(filter);
        var income = query.Where(x => x.Account.AccountType == AccountType.Income).Sum(x => x.Credit - x.Debit);
        var expenses = query.Where(x => x.Account.AccountType == AccountType.Expense).Sum(x => x.Debit - x.Credit);

        return Task.FromResult(new IncomeStatementDto(income, expenses, income - expenses));
    }

    public Task<BalanceSheetDto> GetBalanceSheetAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var query = FilterLedger(filter);
        var assets = query.Where(x => x.Account.AccountType == AccountType.Asset).Sum(x => x.Debit - x.Credit);
        var liabilities = query.Where(x => x.Account.AccountType == AccountType.Liability).Sum(x => x.Credit - x.Debit);
        var income = query.Where(x => x.Account.AccountType == AccountType.Income).Sum(x => x.Credit - x.Debit);
        var expenses = query.Where(x => x.Account.AccountType == AccountType.Expense).Sum(x => x.Debit - x.Credit);

        return Task.FromResult(new BalanceSheetDto(assets, liabilities, assets - liabilities + income - expenses));
    }

    private IQueryable<TransactionRecord> FilterTransactions(ReportFilter filter)
    {
        var query = transactions.Query().Where(x => !x.IsDeleted);
        if (filter.From is not null) query = query.Where(x => x.OccurredAt >= filter.From);
        if (filter.To is not null) query = query.Where(x => x.OccurredAt <= filter.To);
        if (filter.LocationId is not null) query = query.Where(x => x.LocationId == filter.LocationId);
        if (filter.CashierId is not null) query = query.Where(x => x.CashierId == filter.CashierId);
        if (filter.GameModeId is not null) query = query.Where(x => x.GameModeId == filter.GameModeId);
        return query;
    }

    private IQueryable<LedgerEntry> FilterLedger(ReportFilter filter)
    {
        var query = ledgerEntries.Query().Where(x => !x.IsDeleted);
        if (filter.From is not null) query = query.Where(x => x.EntryDate >= filter.From);
        if (filter.To is not null) query = query.Where(x => x.EntryDate <= filter.To);
        if (filter.LocationId is not null) query = query.Where(x => x.LocationId == filter.LocationId);
        if (filter.CashierId is not null) query = query.Where(x => x.TransactionRecord != null && x.TransactionRecord.CashierId == filter.CashierId);
        if (filter.GameModeId is not null) query = query.Where(x => x.TransactionRecord != null && x.TransactionRecord.GameModeId == filter.GameModeId);
        return query;
    }
}

public sealed class MasterDataService(
    IRepository<AppUser> users,
    IRepository<Location> locations,
    IRepository<Cashier> cashiers,
    IRepository<Customer> customers,
    IRepository<Member> members,
    IRepository<GameRegister> games,
    IRepository<Account> accounts,
    IRepository<GameMode> gameModes,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IMasterDataService
{
    public Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserDto>>(users.Query().Where(x => !x.IsDeleted).OrderBy(x => x.UserName)
            .Select(x => new UserDto(x.Id, x.UserName, x.FullName, x.Role, x.LocationId, x.IsActive)).ToList());

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            UserName = request.UserName,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            LocationId = request.LocationId,
            CreatedBy = currentUser.UserName
        };
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.UserName, user.FullName, user.Role, user.LocationId, user.IsActive);
    }

    public Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LocationDto>>(locations.Query().Where(x => !x.IsDeleted).OrderBy(x => x.ClubName).Select(ToDto).ToList());

    public async Task<LocationDto> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Location
        {
            ClubName = request.ClubName,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            Phone = request.Phone,
            Mobile = request.Mobile,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            Manager = request.Manager,
            Caretaker = request.Caretaker,
            CreatedBy = currentUser.UserName
        };
        await locations.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public Task<IReadOnlyList<CashierDto>> GetCashiersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CashierDto>>(cashiers.Query().Where(x => !x.IsDeleted).OrderBy(x => x.CashierCode)
            .Select(x => new CashierDto(x.Id, x.LocationId, x.Location.ClubName, x.CashierCode, x.FullName, x.CashRegisterBalance, x.IsActive)).ToList());

    public async Task<CashierDto> CreateCashierAsync(CreateCashierRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Cashier
        {
            LocationId = request.LocationId,
            AppUserId = request.AppUserId,
            CashierCode = request.CashierCode,
            FullName = request.FullName,
            CreatedBy = currentUser.UserName
        };
        await cashiers.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CashierDto(entity.Id, entity.LocationId, string.Empty, entity.CashierCode, entity.FullName, entity.CashRegisterBalance, entity.IsActive);
    }

    public Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CustomerDto>>(customers.Query().Where(x => !x.IsDeleted).OrderBy(x => x.CustomerCode)
            .Select(x => new CustomerDto(x.Id, x.LocationId, x.Location.ClubName, x.CustomerCode, x.FullName, x.Phone, x.Mobile, x.WhatsApp, x.Email, x.Address, x.ReferralCustomerId, x.Balance, x.BonusPoints, x.IsActive)).ToList());

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Customer
        {
            LocationId = request.LocationId,
            CustomerCode = request.CustomerCode,
            FullName = request.FullName,
            Phone = request.Phone,
            Mobile = request.Mobile,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            Address = request.Address,
            ReferralCustomerId = request.ReferralCustomerId,
            CreatedBy = currentUser.UserName
        };
        await customers.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CustomerDto(entity.Id, entity.LocationId, string.Empty, entity.CustomerCode, entity.FullName, entity.Phone, entity.Mobile, entity.WhatsApp, entity.Email, entity.Address, entity.ReferralCustomerId, entity.Balance, entity.BonusPoints, entity.IsActive);
    }

    public Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MemberDto>>(members.Query().Where(x => !x.IsDeleted).OrderBy(x => x.MembershipNumber)
            .Select(x => new MemberDto(x.Id, x.LocationId, x.CustomerId, x.MembershipNumber, x.MembershipType, x.ExpiryDate, x.IsActive)).ToList());

    public async Task<MemberDto> CreateMemberAsync(CreateMemberRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Member
        {
            LocationId = request.LocationId,
            CustomerId = request.CustomerId,
            MembershipNumber = request.MembershipNumber,
            MembershipType = request.MembershipType,
            ExpiryDate = request.ExpiryDate,
            CreatedBy = currentUser.UserName
        };
        await members.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new MemberDto(entity.Id, entity.LocationId, entity.CustomerId, entity.MembershipNumber, entity.MembershipType, entity.ExpiryDate, entity.IsActive);
    }

    public Task<IReadOnlyList<GameRegisterDto>> GetGamesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<GameRegisterDto>>(games.Query().Where(x => !x.IsDeleted).OrderBy(x => x.GameName)
            .Select(x => new GameRegisterDto(x.Id, x.LocationId, x.GameName, x.NumberOfPlayers, x.PurchaseAmount, x.SupplierInfo, x.MaintenanceContacts, x.MaintenanceCosts, x.LastMaintenanceDate, x.IsActive)).ToList());

    public async Task<GameRegisterDto> CreateGameAsync(CreateGameRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new GameRegister
        {
            LocationId = request.LocationId,
            GameName = request.GameName,
            NumberOfPlayers = request.NumberOfPlayers,
            PurchaseAmount = request.PurchaseAmount,
            SupplierInfo = request.SupplierInfo,
            MaintenanceContacts = request.MaintenanceContacts,
            MaintenanceCosts = request.MaintenanceCosts,
            LastMaintenanceDate = request.LastMaintenanceDate,
            CreatedBy = currentUser.UserName
        };
        await games.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new GameRegisterDto(entity.Id, entity.LocationId, entity.GameName, entity.NumberOfPlayers, entity.PurchaseAmount, entity.SupplierInfo, entity.MaintenanceContacts, entity.MaintenanceCosts, entity.LastMaintenanceDate, entity.IsActive);
    }

    public Task<IReadOnlyList<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AccountDto>>(accounts.Query().Where(x => !x.IsDeleted).OrderBy(x => x.AccountNumber)
            .Select(x => new AccountDto(x.Id, x.AccountNumber, x.AccountName, x.AccountType)).ToList());

    public Task<IReadOnlyList<GameModeDto>> GetGameModesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<GameModeDto>>(gameModes.Query().Where(x => !x.IsDeleted).OrderBy(x => x.Code)
            .Select(x => new GameModeDto(x.Id, x.ModeType, x.Code, x.Name)).ToList());

    private static LocationDto ToDto(Location x) => new(x.Id, x.ClubName, x.Address, x.City, x.State, x.Country, x.Phone, x.Mobile, x.WhatsApp, x.Email, x.Manager, x.Caretaker);
}
