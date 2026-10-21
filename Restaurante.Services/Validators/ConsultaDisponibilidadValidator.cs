using FluentValidation;
using Restaurante.Core.DTOs;
using System;

namespace Restaurante.Services.Validators
{
    public class ConsultaDisponibilidadValidator
        : AbstractValidator<ConsultaDisponibilidadDto>
    {
        public ConsultaDisponibilidadValidator()
        {
            // Debe consultar para al menos una persona.
            RuleFor(x => x.CantidadPersonas)
                .GreaterThan(0)
                .WithMessage("La cantidad de personas debe ser mayor que cero.");

            // Inicio obligatorio y no pasado, en horario de Bolivia.
            RuleFor(x => x.FechaHoraInicio)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("La fecha y hora de inicio es obligatoria.")
                .Must(inicio => inicio >= DateTime.UtcNow.AddHours(-4))
                .WithMessage("La fecha y hora de inicio no debe estar en el pasado.");

            // La finalización debe ser posterior al inicio.
            RuleFor(x => x.FechaHoraFin)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("La fecha y hora de fin es obligatoria.")
                .GreaterThan(x => x.FechaHoraInicio)
                .WithMessage("La fecha y hora de fin debe ser posterior al inicio.");
        }
    }
}