using AutoMapper;
using Restaurante.Core.CustomEntities.Reporteria;
using Restaurante.Core.DTOs.Reporteria;

namespace Restaurante.Infrastructure.Mappings;

public class ReportesProfile : Profile
{
    public ReportesProfile()
    {
        CreateMap<ClienteFrecuente, ClienteFrecuenteDto>();
    }
}
