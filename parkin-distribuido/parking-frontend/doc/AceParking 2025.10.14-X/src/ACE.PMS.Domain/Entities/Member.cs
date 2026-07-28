namespace ACE.PMS.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LicensePlate { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    //所屬車類,ServiceCategory=Monthly 
    public Vehicle Vehicle { get; set; } = null!;
        
    //所屬月租車位組
    //public int? SpaceGroupId { get; set; }
    public SpaceGroup? SpaceGroup { get; set; }

    //允許進入的泊車區域(ZoneId) 列表
    public int[] AllowedZoneIds { get; set; } = [1];

    //車位類別 (如：固定 Reserved、浮動 Floating、有蓋 Covered 露天 Open)
    public SpaceTypes SpaceType { get; set; } = SpaceTypes.Regular;

    //車位編號 (如：A01、B12、M03 ...)
    public string SpaceNo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    //public bool IsDeleted { get; set; }

    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;    
    public string Notes { get; set; } = string.Empty;

    public VehicleTypes VehicleTypeId => Vehicle.VehicleTypeId;

    public Member(string licensePlate, string cardId, DateTime startDate, DateTime expiryDate)
    {
        LicensePlate = licensePlate;
        CardId = cardId;
        StartDate = startDate;
        ExpiryDate = expiryDate;
    }
}
