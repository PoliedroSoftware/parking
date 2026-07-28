using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Parking.Api;

public class ApiAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(principal));
    }
}
