using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Mappings
{
    public class ReservaProfile : Profile
    {
        public ReservaProfile()
        {
            // Convierte la reserva en su DTO de detalle/confirmación.
            CreateMap<Reserva, ReservaDetalleDto>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.TelefonoCliente,
                    opt => opt.MapFrom(src => src.Cliente.Telefono))
                .ForMember(dest => dest.NumeroMesa,
                    opt => opt.MapFrom(src => src.Mesa.Numero))
                .ForMember(dest => dest.Estado,
                    opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.FechaRegistro,
                    opt => opt.MapFrom(src => src.FechaRegistro));
        }
    }
}
