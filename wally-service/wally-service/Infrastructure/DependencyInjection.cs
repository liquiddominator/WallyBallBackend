using Cassandra;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WallyBallBackend.Application.Campeonatos;
using WallyBallBackend.Application.Categorias;
using WallyBallBackend.Application.DatosPrueba;
using WallyBallBackend.Application.Equipos;
using WallyBallBackend.Application.Fixture;
using WallyBallBackend.Application.Jugadores;
using WallyBallBackend.Application.Posiciones;
using WallyBallBackend.Application.PortalJugador;
using WallyBallBackend.Application.Reportes;
using WallyBallBackend.Application.Resultados;
using WallyBallBackend.Infrastructure.Authentication;
using WallyBallBackend.Infrastructure.Campeonatos;
using WallyBallBackend.Infrastructure.Categorias;
using WallyBallBackend.Infrastructure.DatosPrueba;
using WallyBallBackend.Infrastructure.Equipos;
using WallyBallBackend.Infrastructure.Fixture;
using WallyBallBackend.Infrastructure.Jugadores;
using WallyBallBackend.Infrastructure.Persistence.Cassandra;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;
using WallyBallBackend.Infrastructure.Personas;
using WallyBallBackend.Infrastructure.Posiciones;
using WallyBallBackend.Infrastructure.PortalJugador;
using WallyBallBackend.Infrastructure.Reportes;
using WallyBallBackend.Infrastructure.Resultados;

namespace WallyBallBackend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CassandraOptions>(configuration.GetSection(CassandraOptions.SectionName));
        services.Configure<PersonasServiceOptions>(configuration.GetSection(PersonasServiceOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddHttpClient<IPersonasServiceClient, PersonasServiceClient>(client =>
        {
            var options = configuration.GetSection(PersonasServiceOptions.SectionName).Get<PersonasServiceOptions>() ?? new PersonasServiceOptions();
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("SqlServer");
            options.UseSqlServer(connectionString);
        });

        services.AddSingleton<ICluster>(_ =>
        {
            var options = configuration.GetSection(CassandraOptions.SectionName).Get<CassandraOptions>() ?? new CassandraOptions();

            return Cluster.Builder()
                .AddContactPoints(options.ContactPoints)
                .WithPort(options.Port)
                .Build();
        });

        services.AddScoped<ICassandraSessionFactory, CassandraSessionFactory>();
        services.AddScoped<ICampeonatoService, CampeonatoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IDatosPruebaService, DatosPruebaService>();
        services.AddScoped<IEquipoService, EquipoService>();
        services.AddScoped<IFixtureService, FixtureService>();
        services.AddScoped<IJugadorService, JugadorService>();
        services.AddScoped<IPosicionService, PosicionService>();
        services.AddScoped<IPortalJugadorService, PortalJugadorService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IResultadoService, ResultadoService>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authorizationHeader = context.Request.Headers.Authorization.ToString();

                        if (authorizationHeader.StartsWith("Bearer Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorizationHeader["Bearer Bearer ".Length..].Trim();
                        }
                        else if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorizationHeader["Bearer ".Length..].Trim();
                        }

                        context.Token = context.Token?.Trim().Trim('"');

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
