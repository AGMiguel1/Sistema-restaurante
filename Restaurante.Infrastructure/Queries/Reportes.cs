namespace Restaurante.Infrastructure.Queries;

internal static class Reportes
{
    internal const string ClientesFrecuentes = """
        SELECT c.Id AS ClienteId,
               c.Nombre,
               COUNT(r.Id) AS TotalReservas
        FROM clientes c
        INNER JOIN reservas r ON r.ClienteId = c.Id
        WHERE r.Estado IN (@Confirmada, @Completada)
        GROUP BY c.Id, c.Nombre
        ORDER BY TotalReservas DESC, c.Id ASC
        LIMIT @Limite;
        """;
}
