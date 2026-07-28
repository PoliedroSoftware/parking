// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.LoginAudits.DTOs;

[Description("Login Audits")]
public class LoginAuditDto
{
    [Display(Name = "Id")] public int Id { get; set; }
    [Display(Name = "Login Time")] public DateTime LoginTimeUtc { get; set; }
    [Display(Name = "User Id")] public string UserId { get; set; } = string.Empty;
    [Display(Name ="User")] public ApplicationUserDto? User { get; set; }
    [Display(Name = "User Name")] public string UserName { get; set; } = string.Empty;
    [Display(Name = "IP Address")] public string? IpAddress { get; set; }
    [Display(Name = "Browser Info")] public string? BrowserInfo { get; set; }
    [Display(Name = "Region")] public string? Region { get; set; }
    [Display(Name = "Provider")] public string? Provider { get; set; }
    [Display(Name = "Success")] public bool Success { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Domain.Identity.LoginAudit, LoginAuditDto>().ReverseMap();
        }
    }
}
