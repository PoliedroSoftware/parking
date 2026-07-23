using CleanArchitecture.Blazor.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public sealed class CompanyInformationService(IApplicationDbContextFactory dbFactory)
{
    public async Task<TicketCompanyData> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateAsync(cancellationToken);
        var company = await db.CompanyInformation
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return company is null
            ? TicketCompanyData.Default
            : new TicketCompanyData(
                company.DisplayName,
                company.TradeName,
                company.TaxId,
                company.Address,
                company.Phone,
                company.FooterText);
    }

    public async Task ApplyAsync(TicketData ticketData, CancellationToken cancellationToken = default)
    {
        ticketData.Company = await GetAsync(cancellationToken);
    }
}
