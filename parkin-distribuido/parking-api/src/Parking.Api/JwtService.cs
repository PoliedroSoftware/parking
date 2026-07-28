using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Parking.Api;

public class JwtOptions
{
    public const string Section = "Jwt";
    public string Secret { get; set; } = "ParkingApi_SecretKey_ChangeMeInProduction_2024!";
    public string Issuer { get; set; } = "ParkingApi";
    public string Audience { get; set; } = "ParkingClients";
    public int ExpirationMinutes { get; set; } = 480;
}

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles, IList<Claim>? permissions = null);
}

public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateToken(ApplicationUser user, IList<string> roles, IList<Claim>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new("displayName", user.DisplayName ?? ""),
            new("tenantId", user.TenantId ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (permissions is not null)
            claims.AddRange(permissions);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
