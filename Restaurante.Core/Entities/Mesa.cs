using System.Collections.Generic;

namespace Restaurante.Core.Entities;

public partial class Mesa
{
    // id mesa
    public int Id { get; set; }

    // Número de la mesa en el restaurante
    public int Numero { get; set; }

    // Cantidad max de personas
    public int Capacidad { get; set; }

    // Reservas asociadas a esta mesa
    public virtual ICollection<Reserva> Reservas { get; set; }
        = new List<Reserva>();
}