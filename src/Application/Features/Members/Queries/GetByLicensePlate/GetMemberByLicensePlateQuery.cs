#nullable enable
#nullable disable warnings

using CleanArchitecture.Blazor.Application.Features.Members.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Members.Queries.GetByLicensePlate;

public class GetMemberByLicensePlateQuery : IRequest<Result<MemberDto>>
{
    public required string LicensePlate { get; set; }
}

public class GetMemberByLicensePlateQueryHandler : IRequestHandler<GetMemberByLicensePlateQuery, Result<MemberDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public GetMemberByLicensePlateQueryHandler(IMapper mapper, IApplicationDbContextFactory dbContextFactory)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<MemberDto>> Handle(GetMemberByLicensePlateQuery request, CancellationToken cancellationToken)
    {
        var plate = request.LicensePlate.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(plate))
            return await Result<MemberDto>.FailureAsync("La placa es requerida para cobrar a mensualidad.");

        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        var member = await db.Members
            .AsNoTracking()
            .Where(x => x.IsActive && x.LicensePlate != null && x.LicensePlate.ToUpper() == plate)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return member is null
            ? await Result<MemberDto>.FailureAsync($"No se encontro un mensualista activo para la placa {plate}.")
            : await Result<MemberDto>.SuccessAsync(member);
    }
}
