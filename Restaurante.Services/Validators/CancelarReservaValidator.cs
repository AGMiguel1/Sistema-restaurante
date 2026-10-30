using FluentValidation;
using Restaurante.Core.DTOs;

namespace Restaurante.Services.Validators
{
    public class CancelarReservaValidator : AbstractValidator<CancelarReservaDto>
    {
        public CancelarReservaValidator()
        {
            RuleFor(x => x.CanceladaPor)
                .NotEmpty()
                .WithMessage("El nombre o rol de quien cancela es obligatorio.")
                .MaximumLength(100)
                .WithMessage("El campo cancelada por no debe exceder los 100 caracteres.");

            RuleFor(x => x.Motivo)
                .MaximumLength(300)
                .WithMessage("El motivo no debe exceder los 300 caracteres.");
        }
    }
}
