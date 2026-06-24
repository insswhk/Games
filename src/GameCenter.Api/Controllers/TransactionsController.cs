using GameCenter.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController(ITransactionService transactionService, IPermissionService permissions) : ControllerBase
{
    [HttpPost]
    public async Task<TransactionResultDto> Create(CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Transactions", PermissionAction.Add, cancellationToken);
        return await transactionService.CreateAsync(request, cancellationToken);
    }
}
