
namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PermitTypes
{
    [DisplayEn("None")]
    [DisplayTc("未知")]
    None = 0,

    [DisplayEn("Octopus")]
    [DisplayTc("八達通卡")]    
    OctopusCard =  1,
        
    [DisplayEn("Smartcard")]
    [DisplayTc("智能卡")]
    Smartcard =     2,

    [DisplayEn("PlateNo")]    
    [DisplayTc("車牌號碼")]
    LicensePlate =   4,

    [DisplayEn("Ticket")]
    [DisplayTc("磁卡")]
    MagneticTicket = 8,

    [DisplayEn("QRCode")]
    [DisplayTc("QRCode")]
    QRCode =   1 << 4,
}