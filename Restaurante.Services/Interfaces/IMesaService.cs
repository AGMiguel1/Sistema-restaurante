using Restaurante.Core.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public interface IMesaService
    {
        Task<IEnumerable<MesaDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<MesaDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<MesaDetalleDto> CrearAsync(GuardarMesaDto dto, CancellationToken cancellationToken = default);
        Task<MesaDetalleDto> ActualizarAsync(int id, GuardarMesaDto dto, CancellationToken cancellationToken = default);
        Task EliminarAsync(int id, CancellationToken cancellationToken = default);

        Task<IEnumerable<MesaDisponibleDto>> ConsultarDisponibilidadAsync(
            ConsultaDisponibilidadDto consulta,
            CancellationToken cancellationToken = default);
    }
}
