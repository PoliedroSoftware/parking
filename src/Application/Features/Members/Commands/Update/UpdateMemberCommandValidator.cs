#nullable enable
#nullable disable warnings

namespace CleanArchitecture.Blazor.Application.Features.Members.Commands.Update;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(v => v.Id).NotNull();
        RuleFor(v => v.Name).MaximumLength(100).NotEmpty().WithMessage("El nombre es requerido");
        RuleFor(v => v.PhoneNumber).MaximumLength(20);
        RuleFor(v => v.StartDate).NotNull().WithMessage("La fecha de inicio es requerida");
        RuleFor(v => v.LicensePlate).MaximumLength(255);
        RuleFor(v => v.Notes).MaximumLength(500);
    }
}
