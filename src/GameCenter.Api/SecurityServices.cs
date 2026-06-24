using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameCenter.Application;
using GameCenter.Domain;
using Microsoft.IdentityModel.Tokens;

namespace GameCenter.Api;

public sealed class HttpCurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string UserName => accessor.HttpContext?.User.Identity?.Name ?? "system";

    public UserRole? Role
    {
        get
        {
            var role = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(role, out var parsed) ? parsed : null;
        }
    }

    public Guid? LocationId
    {
        get
        {
            var locationId = accessor.HttpContext?.User.FindFirstValue("locationId");
            return Guid.TryParse(locationId, out var parsed) ? parsed : null;
        }
    }
}

public sealed class JwtTokenFactory(IConfiguration configuration) : IJwtTokenFactory
{
    public string CreateToken(AppUser user)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT key is missing.")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("fullName", user.FullName)
        };

        if (user.LocationId is not null)
        {
            claims.Add(new Claim("locationId", user.LocationId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
