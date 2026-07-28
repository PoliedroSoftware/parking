using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

/// <summary>
/// Parking Rate 停車場收費
/// </summary>
public class Charge : BaseAuditableEntity, IMayHaveTenant
{
    public string Name { get; set; } = string.Empty; //收費方案名稱 Name of Rate Plan
    public DateTime EffectiveDate { get; set; } = new DateTime(2025, 8, 1); //生效日期 Effective Date
    public RateContent? BeforeContent { get; set; }
    public RateContent? AfterContent { get; set; }
    public string? Description { get; set; }
    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    //public ICollection<Vehicle> Vehicles { get; set; } = []; // 導航屬性 = new List<Vehicle>();


}


public class RateContent
{
    public static readonly int MaxNormalChargeItems = 5;   //正常收费项目最大栏位数
    public static readonly int MaxSpecialChargeItems = 3;  //特別时段收费项目最大栏位数

    //正常收费 Normal Rate
    public List<ChargeItem> NormalCharges { get; set; } = new();  //(最大5欄位 MaxNormalChargeItems)

    //特别時段收費 Special Rate （如優惠时段,繁忙时段)
    public List<ChargePeriod> SpecialPeriod { get; set; } = new();
    public List<ChargeItem> SpecialCharges { get; set; } = new(); //(最大3欄位 MaxSpecialChargeItems)

    //优惠收费 Reduced Rate
    public ReducedItem DayPark { get; set; } = new();      //日泊優惠
    public ReducedItem NightPark { get; set; } = new();    //夜泊優惠
    public MaxReducedItem Max12Park { get; set; } = new();     //12小时泊
    public MaxReducedItem Max24Park { get; set; } = new();     //24小时泊
    public MaxReducedItem FullDayPark { get; set; } = new();   //全日任泊 (入车时间起计算至同日午夜十二时止)        

}

public class ChargePeriod
{
    public TimeSpan? StartTime { get; set; }  //開始時間
    public TimeSpan? EndTime { get; set; }    //結束時間
}
public class ChargeItem
{
    //收费时长(分钟),平日收费,假日收费
    public int Duration { get; set; }
    public int PriceWeekday { get; set; }
    public int PriceHoliday { get; set; }
}
public class ReducedItem
{
    //時段,平日最高收费,假日最高收费
    public List<ChargePeriod> Period { get; set; }=new List<ChargePeriod>() { new ChargePeriod() };
    public int CeilingAmountWeekday { get; set; }
    public int CeilingAmountHoliday { get; set; }
}
public class MaxReducedItem
{
    public bool IsActive { get; set; }
    public int CeilingAmountWeekday { get; set; }
    public int CeilingAmountHoliday { get; set; } //是否使用,平日最高收费,假日最高收费
}
