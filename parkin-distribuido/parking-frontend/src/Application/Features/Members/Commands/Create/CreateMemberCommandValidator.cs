#nullable enable
#nullable disable warnings

namespace CleanArchitecture.Blazor.Application.Features.Members.Commands.Create;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(v => v.Name).MaximumLength(100).NotEmpty().WithMessage("El nombre es requerido");
        RuleFor(v => v.PhoneNumber).MaximumLength(20).NotEmpty().WithMessage("El telefono es requerido");
        RuleFor(v => v.StartDate).NotNull().WithMessage("La fecha de inicio es requerida");
        RuleFor(v => v.LicensePlate).MaximumLength(255);
        RuleFor(v => v.Notes).MaximumLength(500);
    }
}
