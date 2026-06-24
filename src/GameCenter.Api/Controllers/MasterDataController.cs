using GameCenter.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/master-data")]
public sealed class MasterDataController(IMasterDataService masterData, IPermissionService permissions) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IReadOnlyList<UserDto>> Users(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Users", PermissionAction.Open, cancellationToken);
        return await masterData.GetUsersAsync(cancellationToken);
    }

    [HttpPost("users")]
    public async Task<UserDto> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Users", PermissionAction.Add, cancellationToken);
        return await masterData.CreateUserAsync(request, cancellationToken);
    }

    [HttpGet("locations")]
    public async Task<IReadOnlyList<LocationDto>> Locations(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Locations", PermissionAction.Open, cancellationToken);
        return await masterData.GetLocationsAsync(cancellationToken);
    }

    [HttpPost("locations")]
    public async Task<LocationDto> CreateLocation(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Locations", PermissionAction.Add, cancellationToken);
        return await masterData.CreateLocationAsync(request, cancellationToken);
    }

    [HttpGet("cashiers")]
    public async Task<IReadOnlyList<CashierDto>> Cashiers(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Cashiers", PermissionAction.Open, cancellationToken);
        return await masterData.GetCashiersAsync(cancellationToken);
    }

    [HttpPost("cashiers")]
    public async Task<CashierDto> CreateCashier(CreateCashierRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Cashiers", PermissionAction.Add, cancellationToken);
        return await masterData.CreateCashierAsync(request, cancellationToken);
    }

    [HttpGet("customers")]
    public async Task<IReadOnlyList<CustomerDto>> Customers(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Customers", PermissionAction.Open, cancellationToken);
        return await masterData.GetCustomersAsync(cancellationToken);
    }

    [HttpPost("customers")]
    public async Task<CustomerDto> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Customers", PermissionAction.Add, cancellationToken);
        return await masterData.CreateCustomerAsync(request, cancellationToken);
    }

    [HttpGet("members")]
    public async Task<IReadOnlyList<MemberDto>> Members(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Members", PermissionAction.Open, cancellationToken);
        return await masterData.GetMembersAsync(cancellationToken);
    }

    [HttpPost("members")]
    public async Task<MemberDto> CreateMember(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Members", PermissionAction.Add, cancellationToken);
        return await masterData.CreateMemberAsync(request, cancellationToken);
    }

    [HttpGet("games")]
    public async Task<IReadOnlyList<GameRegisterDto>> Games(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Games", PermissionAction.Open, cancellationToken);
        return await masterData.GetGamesAsync(cancellationToken);
    }

    [HttpPost("games")]
    public async Task<GameRegisterDto> CreateGame(CreateGameRegisterRequest request, CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Games", PermissionAction.Add, cancellationToken);
        return await masterData.CreateGameAsync(request, cancellationToken);
    }

    [HttpGet("accounts")]
    public async Task<IReadOnlyList<AccountDto>> Accounts(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Reports", PermissionAction.ViewReports, cancellationToken);
        return await masterData.GetAccountsAsync(cancellationToken);
    }

    [HttpGet("game-modes")]
    public async Task<IReadOnlyList<GameModeDto>> GameModes(CancellationToken cancellationToken)
    {
        await permissions.EnsureAsync("Transactions", PermissionAction.Open, cancellationToken);
        return await masterData.GetGameModesAsync(cancellationToken);
    }
}
