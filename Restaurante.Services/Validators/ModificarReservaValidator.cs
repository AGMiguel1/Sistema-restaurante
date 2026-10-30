using FluentValidation;
using Restaurante.Core.DTOs;
using System;

namespace Restaurante.Services.Validators
{
    public class ModificarReservaValidator : AbstractValidator<ModificarReservaDto>
    {
        public ModificarReservaValidator()
        {
            RuleFor(x => x.MesaId)
                .GreaterThan(0)
                .WithMessage("El identificador de la mesa debe ser mayor que cero.");

            RuleFor(x => x.CantidadPersonas)
                .GreaterThan(0)
                .WithMessage("La cantidad de personas debe ser mayor que cero.");

            RuleFor(x => x.FechaHoraInicio)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("La fecha y hora de inicio es obligatoria.")
                .Must(inicio => inicio >= DateTime.UtcNow.AddHours(-4))
                .WithMessage("La fecha y hora de inicio no debe estar en el pasado.");

            RuleFor(x => x.FechaHoraFin)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("La fecha y hora de fin es obligatoria.")
                .GreaterThan(x => x.FechaHoraInicio)
                .WithMessage("La fecha y hora de fin debe ser posterior al inicio.");
        }
    }
}
