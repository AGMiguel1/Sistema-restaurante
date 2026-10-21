using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Mappings;
using Restaurante.Infrastructure.Repositories;
using Restaurante.Services.Validators;
using System;

namespace Restaurante.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar la conexión con MySQL.
            var connectionString = builder.Configuration
                .GetConnectionString("ConnectionMySql")
                ?? throw new InvalidOperationException(
                    "No se encontró la conexión ConnectionMySql.");

            builder.Services.AddDbContext<RestauranteContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)));

            // Registrar los repositorios.
            builder.Services.AddScoped<IMesaRepository, MesaRepository>();
            builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

            // Registrar los controladores y OpenAPI.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Registrar los perfiles de AutoMapper.
            builder.Services.AddAutoMapper(typeof(MesaProfile).Assembly);

            // Registrar directamente las clases de los validadores.
            builder.Services.AddScoped<ConsultaDisponibilidadValidator>();
            builder.Services.AddScoped<CrearReservaValidator>();

            var app = builder.Build();

            // Publicar la descripción de la API en desarrollo.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}