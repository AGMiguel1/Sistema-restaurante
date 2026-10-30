namespace Restaurante.Core.DTOs
{
    public class MesaDisponibleDto
    {
        // id mesa
        public int Id { get; set; }

        // Numero con el que se identifica la mesa en el restaurante
        public int Numero { get; set; }

        // Cantidad max de personas que pueden ocupar la mesa
        public int Capacidad { get; set; }
    }
}