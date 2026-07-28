using System.Security.Claims;
using CleanArchitecture.Blazor.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Parking.Api.DTOs;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService,
    IPermissionService permissionService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username)
                   ?? await userManager.FindByEmailAsync(request.Username);

        if (user is null)
            return Unauthorized(new { error = "Usuario o contrasena invalidos" });

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Unauthorized(new { error = "Cuenta bloqueada. Intente mas tarde." });

        if (!result.Succeeded)
            return Unauthorized(new { error = "Usuario o contrasena invalidos" });

        var roles = await userManager.GetRolesAsync(user);
        var allPermissions = permissionService.GetAllPermissions();

        var token = jwtService.GenerateToken(user, roles, allPermissions.Select(p => new Claim(ApplicationClaimTypes.Permission, p)).ToList());

        return Ok(new LoginResponse(
            token,
            user.DisplayName ?? user.UserName ?? "",
            user.Email ?? "",
            roles.ToArray()));
    }

    [HttpGet("permissions"), Authorize]
    public async Task<ActionResult<object>> GetPermissions()
    {
        var permissions = await permissionService.GetUserPermissionsAsync();
        var allPermissions = permissionService.GetAllPermissions();
        return Ok(new { permissions, all = allPermissions });
    }
}
