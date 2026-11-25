using FluentValidation;
using Restaurante.Core.DTOs;

namespace Restaurante.Services.Validators;

public class GuardarMesaValidator : AbstractValidator<GuardarMesaDto>
{
    public GuardarMesaValidator()
    {
        RuleFor(x => x.Numero).GreaterThan(0);
        RuleFor(x => x.Capacidad).GreaterThan(0);
    }
}
