using Asp.Versioning;
using Cassandra;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ReportesService.Application.Reportes;
using ReportesService.Infrastructure.Authentication;
using ReportesService.Infrastructure.Cassandra;
using ReportesService.Infrastructure.Reportes;
using Serilog;
using System.Security.Claims;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            var origins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:5174"];

            policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Reportes Service API",
            Version = "v1",
            Description = "API de reportes de WallyBall respaldada por Cassandra."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pegue solo el JWT. Swagger enviara Authorization: Bearer {token}."
        });
    });

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.Configure<CassandraOptions>(builder.Configuration.GetSection(CassandraOptions.SectionName));

    builder.Services.AddSingleton<ICluster>(_ =>
    {
        var options = builder.Configuration.GetSection(CassandraOptions.SectionName).Get<CassandraOptions>() ?? new CassandraOptions();

        return Cluster.Builder()
            .AddContactPoints(options.ContactPoints)
            .WithPort(options.Port)
            .Build();
    });

    builder.Services.AddScoped<ICassandraSessionFactory, CassandraSessionFactory>();
    builder.Services.AddScoped<IReporteQueryService, CassandraReporteQueryService>();

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

    builder.Services
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

    builder.Services.AddAuthorization();

    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reportes Service API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Reportes Service API";
        });
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "The API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
