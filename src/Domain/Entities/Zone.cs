using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

/// <summary>
/// 停車場Site(泊車區)，内部可有一個或多個泊車區，Id=1為主（外）場或出入通道，如有均為内場(Id>1)。
/// 外場或出入通道不可刪除。如有内場，車需經外場（出入通道）進入后，才可進入内場，離開同理。
/// </summary>
public class Zone : BaseAuditableEntity
{
    /// <summary>
    /// 基本信息 Basic Information
    /// </summary>

    // 區域名稱（出入口名稱） Zone Name (如: Main Zone, Car Park 1/F, Loading Bay Area ...)
    public  MultiCodeName Name { get; set; }
    public bool IsMain { get; set; }
    // 所屬停車場 Carpark
    public  int CarparkId { get; set; }
    public virtual  Carpark Carpark { get; set; }

    // 假期規則 Holiday Options  平日：星期一至星期五(公众假期除外) "Mon-Fri,Public Holiday excluded"  假日：星期六，日及公众假期 "Sat,Sun,PH"    
    public string HolidaySets { get; set; } = "1,0,0,0,0,0,1,1"; //PH,Mon,Tue,Wed,Thu,Fri,Sat,Sun (1=Holiday,0=Not Holiday)            

    //是否開啟錢箱
    public bool IsOpenCashbox { get; set; } = true;

    // 區域内可用車輛 Vehicles Allowed in the Zone
    public virtual ICollection<Vehicle> Vehicles { get; set; } = [];

    public virtual ICollection<SpaceGroup> SpaceGroups { get; set; } = [];

    // 車場出入口閘機集合，控制車輛進出。
    public virtual ICollection<Gate> Gates { get; set; } = [];

    // 区域描述 Description
    public string? Description { get; set; }


    // 時租泊車服務設定 Hourly Parking Service Settings
    public HourlySets HourlySets { get; set; } = new();

    // 月租泊車服務設定 Monthly Parking Service Settings
    public MonthlySets MonthlySets { get; set; } = new();


    
}


public class HourlySets
{
    // 許可的時租通行證類型 Hourly Permit Types Allowed
    public IEnumerable<PermitTypes>? Permits { get; set; } =new HashSet<PermitTypes>(){PermitTypes.OctopusCard };

    // 時租可泊車位數 Hourly Parking Capacity
    public int Capacity { get; set; } = 100;

    // 臨時車位數調整 Temporary Capacity Adjustment (+/-)
    public int Adjustment { get; set; }

    // 手動滿 No Entry Manually
    public bool ManualFull { get; set; } = false;

    // 時租收費豁免時間 Hourly Free Grace Period
    public int GracePeriod { get; set; } = 15;

    // 時租收費后離場時限 Hourly Exit Buffer
    public int ExitBuffer { get; set; } = 15;

    // 失票罰款 Lost Ticket Fee
    public decimal? LostTicketFee { get; set; } = 300;

    // 時租出入車牌驗證 Hourly License Plate Verification Requirement
    public IEnumerable<LicensePlateVerifications>? PlateVerifications { get; set; } = new HashSet<LicensePlateVerifications>();
}


public class MonthlySets
{
    // 允許的月租通行證類型 Monthly Permit Types Allowed
    public IEnumerable<PermitTypes>? Permits { get; set; } = [PermitTypes.Smartcard, PermitTypes.LicensePlate];

    // 月租出入循環檢測 Anti-Passback Control,防止同一證件重複進出
    public IEnumerable<AntiPassbackControls>? AntiPassbacks { get; set; } = [AntiPassbackControls.EntryExit];

    // 月租出入車牌驗證 Monthly License Plate Verification Requirement
    public IEnumerable<LicensePlateVerifications>? PlateVerifications { get; set; } = [LicensePlateVerifications.EntryExit];

    // 無效月租自動轉為時租服务 Monthly Conversion to Hourly when Monthly is not valid
    public IEnumerable<MonthlyConversions>? Conversions { get; set; } = [];

    // 按金金額 Monthly Deposit Amount
    public decimal Deposit { get; set; } = 150m;
}
