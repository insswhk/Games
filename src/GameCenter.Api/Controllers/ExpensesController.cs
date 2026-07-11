using GameCenter.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/expenses")]
public sealed class ExpensesController(IExpenseService expenseService, IPermissionService permissions) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ExpenseDto>> Get(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Expenses", PermissionAction.Open, cancellationToken);
        return await expenseService.GetAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ExpenseDto> Create(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Expenses", PermissionAction.Add, cancellationToken);
        return await expenseService.CreateAsync(request, cancellationToken);
    }
}
