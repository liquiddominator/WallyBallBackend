using Asp.Versioning;
using PersonasService.Api.OpenApi;
using PersonasService.Application;
using PersonasService.Infrastructure;
using PersonasService.Infrastructure.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using System.Threading.RateLimiting;

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

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("auth", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Personas Service API",
            Version = "v1",
            Description = "API de personas, autenticacion y autorizacion del sistema WallyBall."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pegue solo el JWT. Swagger enviara el header Authorization: Bearer {token}."
        });

        options.DocumentFilter<AuthorizeDocumentFilter>();
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

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsEnvironment("Docker"))
    {
        await ApplyDatabaseMigrationsAsync(app);
    }

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Personas Service API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Personas Service API";
        });
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var urls = app.Urls.Count > 0
            ? app.Urls
            : ["http://localhost:5097"];

        foreach (var url in urls)
        {
            Log.Information("Personas API available at {Url}", url);
            Log.Information("Swagger UI: {Url}/swagger", url.TrimEnd('/'));
            Log.Information("Swagger JSON: {Url}/swagger/v1/swagger.json", url.TrimEnd('/'));
            Log.Information("OpenAPI JSON: {Url}/openapi/v1.json", url.TrimEnd('/'));
        }
    });

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "The Personas API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 30;
    var delay = TimeSpan.FromSeconds(5);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            Log.Information("Applying identity SQL Server migrations. Attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
            await dbContext.Database.MigrateAsync();
            Log.Information("Identity SQL Server migrations applied successfully.");
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            Log.Warning(
                exception,
                "Identity SQL Server migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds} seconds.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
    }

    using var finalScope = app.Services.CreateScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<IdentityDbContext>();

    Log.Information("Applying identity SQL Server migrations. Final attempt {MaxAttempts}/{MaxAttempts}", maxAttempts);
    await finalDbContext.Database.MigrateAsync();
    Log.Information("Identity SQL Server migrations applied successfully.");
}
