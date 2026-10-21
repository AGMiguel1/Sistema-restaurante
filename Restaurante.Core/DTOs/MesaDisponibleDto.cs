namespace Restaurante.Core.DTOs
{
    public class MesaDisponibleDto
    {
        // Identificador único de la mesa.
        public int Id { get; set; }

        // Número con el que se identifica la mesa en el restaurante.
        public int Numero { get; set; }

        // Cantidad máxima de personas que pueden ocupar la mesa.
        public int Capacidad { get; set; }
    }
}