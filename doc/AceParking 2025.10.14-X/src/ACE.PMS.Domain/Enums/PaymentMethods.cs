
namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethods
{
    [DisplayEn("None")]
    [DisplayTc("未知")]
    None = 0,

    [DisplayEn("Octopus")]
    [DisplayTc("八達通")]    
    Octopus =  1,
        
    [DisplayEn("CreditCard")]
    [DisplayTc("信用卡")]
    CreditCard =     2,

    [DisplayEn("Cash")]
    [DisplayTc("現金")]
    LicensePlate =   4,

    [DisplayEn("Cheque")]
    [DisplayTc("支票")]
    MagneticTicket = 8,

    [DisplayEn("FPS")]
    [DisplayTc("轉數快")]
    FPS =   1 << 4,

    [DisplayEn("AliPay")]
    [DisplayTc("支付寶")]
    AliPay = 1 << 5,

    [DisplayEn("WeChat Pay")]
    [DisplayTc("微信")]
    WeChatPay = 1 << 6,
}