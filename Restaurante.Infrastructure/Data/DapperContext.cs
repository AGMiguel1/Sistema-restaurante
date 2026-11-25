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
    public Task<IEnumerable<TResult>> QueryAsync<TFirst, TSecond, TThird, TResult>(
        string sql,
        object parameters,
        Func<TFirst, TSecond, TThird, TResult> map,
        string splitOn,
        CancellationToken cancellationToken)
    {
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return context.Database.GetDbConnection().QueryAsync(command, map, splitOn);
    }
}
