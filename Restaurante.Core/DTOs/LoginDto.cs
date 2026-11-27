using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurante.Core.DTOs
{
    public class LoginDto
    {
        public string Correo { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
