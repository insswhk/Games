using GameCenter.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IPermissionService permissionService, ICurrentUserService currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        authService.LoginAsync(request, cancellationToken);

    [Authorize]
    [HttpGet("permissions")]
    public Task<IReadOnlyList<PermissionDto>> Permissions(CancellationToken cancellationToken)
    {
        if (currentUser.Role is null)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        return permissionService.GetRolePermissionsAsync(currentUser.Role.Value, cancellationToken);
    }
}
