using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Mappings
{
    public class ReservaProfile : Profile
    {
        public ReservaProfile()
        {
            // Convierte la reserva en su DTO de confirmacion
            CreateMap<Reserva, ReservaDetalleDto>()

                // Obtiene el nombre desde el cliente relacionado
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente.Nombre))

                // Obtiene el número desde la mesa relacionada
                .ForMember(dest => dest.NumeroMesa,
                    opt => opt.MapFrom(src => src.Mesa.Numero))

                // Convierte el estado a texto, a confirmada 
                .ForMember(dest => dest.Estado,
                    opt => opt.MapFrom(src => src.Estado.ToString()));
        }
    }
}