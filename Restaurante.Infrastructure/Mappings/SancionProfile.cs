using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Mappings
{
    public class SancionProfile : Profile
    {
        public SancionProfile()
        {
            CreateMap<Sancion, SancionDto>()
                .ForMember(dest => dest.MotivoTexto,
                    opt => opt.MapFrom(src => ((MotivoSancion)src.Motivo).ToString()));
        }
    }
}

