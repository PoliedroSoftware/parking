// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.LoginAudits.DTOs;

[Description("User Login Risk Summary")]
public class UserLoginRiskSummaryDto
{
    [Display(Name = "Id")] public int Id { get; set; }
    [Display(Name = "User Id")] public string UserId { get; set; } = string.Empty;
    [Display(Name = "User Name")] public string UserName { get; set; } = string.Empty;
    [Display(Name = "Risk Level")] public SecurityRiskLevel RiskLevel { get; set; }
    [Display(Name = "Risk Score")] public int RiskScore { get; set; }
    [Display(Name = "Description")] public string? Description { get; set; }
    [Display(Name = "Advice")] public string? Advice { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Domain.Identity.UserLoginRiskSummary, UserLoginRiskSummaryDto>().ReverseMap();
        }
    }
} 
