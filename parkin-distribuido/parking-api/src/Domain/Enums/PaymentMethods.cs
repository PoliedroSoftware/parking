using System.ComponentModel;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum PaymentMethods
{
    [Description("Ninguno")]
    None = 0,

    [Description("Efectivo")]
    Cash = 1,

    [Description("Tarjeta Debito/Credito")]
    CreditCard = 2,

    [Description("Transferencia")]
    Transferencia = 4,

    [Description("Nequi")]
    Nequi = 8,

    [Description("Daviplata")]
    Daviplata = 16,

    [Description("A Mensualidad")]
    ChargeToMonthly = 32,

    [Description("Billetera Digital")]
    DigitalWallet = 64,
}
