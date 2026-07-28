using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using CleanArchitecture.Blazor.Domain.Identity;
using Parking.Api;

namespace Parking.Api.UnitTests;

public sealed class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ContainsIdentityAndTenantClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "unit-test-secret-that-is-long-enough-123456789",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "Administrator",
            Email = "admin@example.com",
            DisplayName = "Admin",
            TenantId = "tenant-1"
        };

        var token = new JwtService(options).GenerateToken(user, ["Admin"]);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("user-1", jwt.Subject);
        Assert.Equal("Administrator", jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal("tenant-1", jwt.Claims.Single(x => x.Type == "tenantId").Value);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains(jwt.Audiences, audience => audience == "test-audience");
    }
}
