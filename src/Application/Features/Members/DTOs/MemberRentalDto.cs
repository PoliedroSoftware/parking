using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Members.DTOs;

public class MemberRentalDto
{
    public int Id { get; set; }
    public int? MemberId { get; set; }
    public MemberDto? Member { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public required DateTime StartDate { get; set; }
    public required DateTime ExpiryDate { get; set; }

    //本期月租租金
    public decimal RentalFee { get; set; }

    //本期按金（如有） 
    public decimal Deposit { get; set; }

    //本期應繳費用（可包含租金、按金、其他費用）。
    public decimal AmountDue { get; set; }

    //本期實際繳費金額
    public decimal AmountPaid { get; set; }

    // 繳費時間
    public DateTime PaymentTime { get; set; } = DateTime.Now;

    public PaymentMethods PaymentMethodId { get; set; } = PaymentMethods.None;
    public string? Notes { get; set; }

}