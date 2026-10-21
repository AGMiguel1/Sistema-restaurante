using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Mappings
{
    // Hereda de Profile para definir los mapeos.
    public class MesaProfile : Profile
    {
        public MesaProfile()
        {
            // Convierte una Mesa en MesaDisponibleDto.
            // Id, Numero y Capacidad coinciden por nombre.
            CreateMap<Mesa, MesaDisponibleDto>();
        }
    }
}