using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Mappings
{
    public class ReservaProfile : Profile
    {
        public ReservaProfile()
        {
            // Convierte la reserva en su DTO de confirmación / detalle
            CreateMap<Reserva, ReservaDetalleDto>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombre : string.Empty))
                .ForMember(dest => dest.NumeroMesa,
                    opt => opt.MapFrom(src => src.Mesa != null ? src.Mesa.Numero : 0))
                .ForMember(dest => dest.Estado,
                    opt => opt.MapFrom(src => src.Estado.ToString()));
        }
    }
}
