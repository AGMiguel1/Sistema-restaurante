using System.Net.Mail;
using FluentValidation;
using Restaurante.Core.DTOs;

namespace Restaurante.Services.Validators;

public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
{
    public RegistroUsuarioValidator()
    {
        RuleFor(dto => dto.ClienteId).GreaterThan(0);
        RuleFor(dto => dto.Correo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(150)
            .Must(correo => MailAddress.TryCreate(correo.Trim(), out var direccion) &&
                            direccion.Address == correo.Trim())
            .WithMessage("El correo no es válido.");
        RuleFor(dto => dto.Password).NotEmpty().Length(8, 128);
    }
}
