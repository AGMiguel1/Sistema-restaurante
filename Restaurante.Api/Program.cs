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

            // 1. Configurar conexión MySQL con Pomelo EF Core.
            var connectionString = builder.Configuration
                .GetConnectionString("ConnectionMySql")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión ConnectionMySql.");

            ServerVersion serverVersion;
            try
            {
                serverVersion = ServerVersion.AutoDetect(connectionString);
            }
            catch
            {
                // Fallback seguro en caso de que MySQL no responda o la contraseña esté pendiente de ajuste
                serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
            }

            builder.Services.AddDbContext<RestauranteContext>(options =>
                options.UseMySql(connectionString, serverVersion));

            // 2. Registrar el repositorio genérico (patrón de SocialMediaStudent).
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // 3. Registrar los repositorios específicos.
            builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
            builder.Services.AddScoped<IMesaRepository, MesaRepository>();
            builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
            builder.Services.AddScoped<ICancelacionRepository, CancelacionRepository>();
            builder.Services.AddScoped<ISancionRepository, SancionRepository>();

            // 4. Registrar la capa de servicios (Lógica de Negocio).
            builder.Services.AddScoped<IMesaService, MesaService>();
            builder.Services.AddScoped<IReservaService, ReservaService>();

            // 5. Configurar controladores suprimiendo el filtro automático para que el middleware maneje validaciones.
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // 6. OpenAPI / Swagger.
            builder.Services.AddOpenApi();

            // 7. AutoMapper.
            builder.Services.AddAutoMapper(typeof(MesaProfile).Assembly);

            // 8. Validadores de FluentValidation.
            builder.Services.AddScoped<ConsultaDisponibilidadValidator>();
            builder.Services.AddScoped<CrearReservaValidator>();
            builder.Services.AddScoped<ModificarReservaValidator>();
            builder.Services.AddScoped<CancelarReservaValidator>();

            var app = builder.Build();

            // 9. Middleware global de captura de excepciones (SocialMediaStudent).
            app.UseMiddleware<ExceptionHandlingMiddleware>();

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
