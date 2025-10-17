using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

/// <summary>
/// 車道閘機，分入口/出口，同一車道標準配置為一臺閘機，也可配置高+低機或左+右機配置。
/// </summary>
public class Gate : BaseAuditableEntity
{
    // 閘機名稱 Gate Name (如: Staff Entrance, Loading Bay Exit ...)
    public string Name { get; set; }

    // 所屬停車場區域 Zone (如: Main Zone, Car Park 1/F, Loading Bay Area ...)
    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; } 

    // 閘機類別 Gate Type (Entry/Exit)
    public GateType GateType { get; set; }

    // 車道編號 (1,2,3...9) 如高+低機配置，應為同一車道編號。
    public int LaneNo { get; set; } = 1;

    // 是否為高機 (高肽車)
    public bool IsUpper { get; set; }

    // 是否為左機 (左肽車)
    public bool IsLefthand { get; set; }

    //允許的時租通行證類型（如車牌、智能卡、八達通等）， Hourly Permit Types Allowed
    public List<PermitTypes>? HourlyPermitTypes { get; set; } = [PermitTypes.OctopusCard];

    //允許的月租通行證類型（如車牌、智能卡、八達通等）， Monthly Permit Types Allowed
    public List<PermitTypes>? MonthlyPermitTypes { get; set; } = [PermitTypes.Smartcard, PermitTypes.LicensePlate];


    // 是否啟用 (True/False)
    public bool IsActive { get; set; } = true;

    // 閘機描述 Gate Description
    public string? Description { get; set; }

    public string Identifier => $"{(GateType == GateType.Entry ? "EN" : "EX")}.{LaneNo:00}{(IsUpper ? 2 : 1)}"; //EN01.1, EX01.1, EN02.1, ...


    
}
