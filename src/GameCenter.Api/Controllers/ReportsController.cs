using GameCenter.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(IReportingService reports, IPermissionService permissions) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<DashboardKpiDto> Dashboard([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Dashboard", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetDashboardAsync(filter, cancellationToken);
    }

    [HttpGet("cashier-cash-register")]
    public async Task<IReadOnlyList<CashierCashRegisterReportRow>> CashierCashRegister([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetCashierCashRegisterAsync(filter, cancellationToken);
    }

    [HttpGet("bonus-points-summary")]
    public async Task<IReadOnlyList<BonusPointsReportRow>> BonusPointsSummary([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("BonusPoints", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetBonusPointsSummaryAsync(filter, cancellationToken);
    }

    [HttpGet("general-ledger")]
    public async Task<IReadOnlyList<LedgerReportRow>> GeneralLedger([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetGeneralLedgerAsync(filter, cancellationToken);
    }

    [HttpGet("income-statement")]
    public async Task<IncomeStatementDto> IncomeStatement([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetIncomeStatementAsync(filter, cancellationToken);
    }

    [HttpGet("profit-loss")]
    public async Task<IncomeStatementDto> ProfitLoss([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetIncomeStatementAsync(filter, cancellationToken);
    }

    [HttpGet("balance-sheet")]
    public async Task<BalanceSheetDto> BalanceSheet([FromQuery] ReportFilter filter, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await reports.GetBalanceSheetAsync(filter, cancellationToken);
    }
}
