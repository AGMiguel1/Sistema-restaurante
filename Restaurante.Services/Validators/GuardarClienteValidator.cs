using FluentValidation;
using Restaurante.Core.DTOs;

namespace Restaurante.Services.Validators;

public class GuardarClienteValidator : AbstractValidator<GuardarClienteDto>
{
    public GuardarClienteValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Correo).MaximumLength(150).EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));
    }
}
