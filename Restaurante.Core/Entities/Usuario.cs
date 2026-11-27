using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurante.Core.Entities
{
    public class Usuario : BaseEntity
    {
        public const string PrefijoPasswordTexto = "texto:";

        public string Correo { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public bool Activo { get; set; } = true;

        public int? ClienteId { get; set; }

        public virtual Cliente? Cliente { get; set; }
    }
    }
