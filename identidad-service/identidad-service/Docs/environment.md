# Entorno

## Runtime y SDK

- .NET SDK 10.
- ASP.NET Core Web API.
- Docker Desktop o Docker Engine para ejecucion contenerizada.
- SQL Server para persistencia transaccional de identidad.

## Dependencias NuGet

- `Microsoft.AspNetCore.OpenApi`: documento OpenAPI nativo.
- `Microsoft.EntityFrameworkCore.SqlServer`: proveedor EF Core para SQL Server.
- `Microsoft.EntityFrameworkCore.Design`: herramientas de diseno EF Core.
- `Microsoft.EntityFrameworkCore.Tools`: comandos EF Core.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: validacion JWT.
- `System.IdentityModel.Tokens.Jwt`: utilidades JWT.
- `FluentValidation.DependencyInjectionExtensions`: registro de validadores.
- `Asp.Versioning.Mvc`: versionado de API.
- `Asp.Versioning.Mvc.ApiExplorer`: metadata para Swagger.
- `Serilog.AspNetCore`: logging estructurado.
- `Serilog.Sinks.Console`: logs a consola.
- `Swashbuckle.AspNetCore`: Swagger UI.

## Configuracion

Archivos disponibles:

- `appsettings.json`: configuracion base.
- `appsettings.Development.json`: configuracion local.
- `appsettings.Docker.json`: configuracion para contenedor.

## Variables Principales

```json
"ConnectionStrings": {
  "SqlServer": "Server=localhost,1433;Database=IdentidadDb;..."
}
```

```json
"Jwt": {
  "Issuer": "IdentidadService",
  "Audience": "WallyBallClients",
  "SigningKey": "development-signing-key-change-before-production-32",
  "ExpirationMinutes": 120,
  "RefreshTokenExpirationDays": 7,
  "MaxFailedLoginAttempts": 5,
  "LockoutMinutes": 15
}
```

## Docker

Variables principales usadas por `docker-compose.yml`:

```text
ASPNETCORE_ENVIRONMENT=Docker
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__SqlServer=Server=identidad-sqlserver,1433;Database=IdentidadDb;...
Jwt__Issuer=IdentidadService
Jwt__Audience=WallyBallClients
```

## Puertos

- API local: `5097`.
- API en Docker: `5097 -> 8080`.
- SQL Server standalone del servicio: `1434 -> 1433`.

## Observabilidad

Serilog queda configurado para salida por consola. En fases posteriores se puede agregar correlacion distribuida entre microservicios.
