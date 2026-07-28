using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

/// <summary>
/// 停車場信息 General Information of Car Park
/// </summary>
public class Carpark : BaseAuditableEntity,IMayHaveTenant
{
    /// <summary>
    /// 注冊信息 Registration Information
    /// </summary>
    public string AppKey { get; init; } = Guid.CreateVersion7().ToString();
    public string? MachineCode { get; set; }
    public string? RegistrationCode { get; set; }

    /// <summary>
    /// 關於車場 About Car Park
    /// </summary>
    // Car Park Name 停車場名稱
    public required MultiCodeName Name { get; set; }

    // Car Park Address 停車場地址
    public MultiName Address { get; set; } = new("", "");

    // Company Name 公司名稱
    public MultiName CompanyName { get; set; } = new("", "");

    // 聯絡人 Contact Person
    public string? ContactPerson { get; set; }

    // 電話 PhoneNumber
    public string? PhoneNumber { get; set; }

    // 傳真 Fax
    public string? Fax { get; set; }

    // 電郵地址 Email
    public string? Email { get; set; }

    // 説明 Description
    public string? Description { get; set; }

    public virtual ICollection<Zone> Zones { get; set; } = [];

    public string Identified => $"P{Id}";

    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
