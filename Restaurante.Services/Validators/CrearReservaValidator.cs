using FluentValidation;
using Restaurante.Core.DTOs;
using System;

namespace Restaurante.Services.Validators
{
    public class CrearReservaValidator : AbstractValidator<CrearReservaDto>
    {
        public CrearReservaValidator()
        {
            // La mesa debe tener un identificador válido.
            RuleFor(x => x.MesaId)
                .GreaterThan(0)
                .WithMessage("El identificador de la mesa debe ser mayor que cero.");

            // Nombre obligatorio y compatible con la columna de MySQL.
            RuleFor(x => x.NombreCliente)
                .NotEmpty()
                .WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(150)
                .WithMessage("El nombre no puede exceder los 150 caracteres.");

            // Teléfono obligatorio.
            RuleFor(x => x.TelefonoCliente)
                .NotEmpty()
                .WithMessage("El teléfono del cliente es obligatorio.")
                .MaximumLength(30)
                .WithMessage("El teléfono no puede exceder los 30 caracteres.");

            // El correo es opcional; se valida cuando se proporciona.
            RuleFor(x => x.CorreoCliente)
                .EmailAddress()
                .WithMessage("El formato del correo electrónico es inválido.")
                .MaximumLength(150)
                .WithMessage("El correo no puede exceder los 150 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CorreoCliente));

            // Debe asistir al menos una persona.
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