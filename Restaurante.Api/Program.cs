using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Filters;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Mappings;
using Restaurante.Infrastructure.Repositories;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Services;
using Restaurante.Services.Validators;
using System;

namespace Restaurante.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configurar BD MySQL
            var connectionString = builder.Configuration
                .GetConnectionString("ConnectionMySql")
                ?? throw new InvalidOperationException(
                    "No se encontró la conexión ConnectionMySql.");

            builder.Services.AddDbContext<RestauranteContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)));
            #endregion

            #region Registro de Repositorios
            // Repositorio genérico para operaciones comunes
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Repositorios específicos para consultas especializadas
            builder.Services.AddScoped<IMesaRepository, MesaRepository>();
            builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
            #endregion

            #region Registro de Servicios
            builder.Services.AddScoped<IMesaService, MesaService>();
            builder.Services.AddScoped<IReservaService, ReservaService>();
            #endregion

            #region Controladores y Configuración de API
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddOpenApi();
            #endregion

            #region AutoMapper y Validadores
            builder.Services.AddAutoMapper(typeof(MesaProfile).Assembly);

            builder.Services.AddScoped<ConsultaDisponibilidadValidator>();
            builder.Services.AddScoped<CrearReservaValidator>();
            #endregion

            var app = builder.Build();

            #region Pipeline HTTP y Middleware de Excepciones
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
