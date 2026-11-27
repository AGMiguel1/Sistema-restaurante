using System.Net;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs.Reporteria;
using Restaurante.Core.Exceptions;
using Restaurante.Services.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController(IReportesService reportesService) : ControllerBase
{
    [HttpGet("dapper/clientes-frecuentes")]
    public async Task<IActionResult> ObtenerClientesFrecuentes([FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            throw new BusinessException("El límite debe ser un número entero.", HttpStatusCode.BadRequest);

        var clientes = await reportesService.ObtenerClientesFrecuentesAsync(limite, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<ClienteFrecuenteDto>>(clientes));
    }
}
