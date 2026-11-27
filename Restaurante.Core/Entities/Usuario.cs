using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurante.Core.Entities
{
    public class Usuario : BaseEntity
    {
        public const string PrefijoPasswordSinHash = "texto:";

        public string Correo { get; set; } = null!;

        // La columna existente guarda hashes anteriores o contraseñas marcadas como texto.
        public string PasswordHash { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public bool Activo { get; set; } = true;

        public int? ClienteId { get; set; }

        public virtual Cliente? Cliente { get; set; }
    }
    }
