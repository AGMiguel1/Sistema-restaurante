using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Data;

public class DapperContext(RestauranteContext context)
{
    public Task<IEnumerable<TResult>> QueryAsync<TResult>(
        string sql, object parameters, CancellationToken cancellationToken)
        => context.Database.GetDbConnection().QueryAsync<TResult>(CreateCommand(sql, parameters, cancellationToken));

    public Task<IEnumerable<TResult>> QueryAsync<TFirst, TSecond, TThird, TResult>(
        string sql,
        object parameters,
        Func<TFirst, TSecond, TThird, TResult> map,
        string splitOn,
        CancellationToken cancellationToken)
    {
        return context.Database.GetDbConnection().QueryAsync(CreateCommand(sql, parameters, cancellationToken), map, splitOn);
    }

    private CommandDefinition CreateCommand(string sql, object parameters, CancellationToken cancellationToken)
        => new(sql, parameters, context.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);
}
