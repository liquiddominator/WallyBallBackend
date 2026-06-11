# Entorno de Trabajo

Este documento describe dependencias, servicios externos y configuracion de `wally-service`.

## Runtime y SDK

- .NET SDK 10: necesario para compilar y ejecutar el proyecto.
- ASP.NET Core Web API: framework base para endpoints HTTP.
- Docker Desktop o Docker Engine: necesario para ejecutar API y SQL Server con `docker compose`.

## Dependencias NuGet Instaladas

- `Microsoft.AspNetCore.OpenApi`: documento OpenAPI nativo.
- `Microsoft.EntityFrameworkCore.SqlServer`: proveedor EF Core para SQL Server.
- `Microsoft.EntityFrameworkCore.Design`: herramientas de diseno EF Core.
- `Microsoft.EntityFrameworkCore.Tools`: comandos EF Core.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: middleware para validar JWT emitidos por `personas-service`.
- `System.IdentityModel.Tokens.Jwt`: utilidades para validar JWT.
- `FluentValidation.DependencyInjectionExtensions`: registro automatico de validadores.
- `Asp.Versioning.Mvc`: versionado de API.
- `Asp.Versioning.Mvc.ApiExplorer`: metadata de versionado para Swagger.
- `Serilog.AspNetCore`: logging estructurado.
- `Serilog.Sinks.Console`: salida de logs a consola.
- `Swashbuckle.AspNetCore`: Swagger UI.

## Servicios Externos Esperados

### SQL Server

Uso previsto:

- Persistencia principal de campeonatos, categorias, equipos, jugadores, partidos, resultados y auditoria.
- Ejecucion de migraciones EF Core.
- Ejecucion opcional del script `wally-service/docs/database/sqlserver/WallyBallDbScript.txt`.

Cadena de conexion local:

```json
"SqlServer": "Server=localhost,1433;Database=WallyBallDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False"
```

### Personas Service

Uso previsto:

- Emitir JWT.
- Administrar usuarios, roles, refresh tokens y sesiones.
- Proveer tokens que `wally-service` valida en endpoints protegidos.

La seccion `Jwt` de ambos servicios debe compartir emisor, audiencia y llave de firma mientras se use firma simetrica en desarrollo.

## Configuracion Local

Archivos disponibles:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Docker.json`

Secciones principales:

- `ConnectionStrings:SqlServer`: conexion a SQL Server.
- `Jwt`: parametros para validar tokens emitidos por personas-service.
- `Serilog`: nivel y destino de logs.

## Configuracion Docker

Archivos:

- `Dockerfile`
- `.dockerignore`
- `docker-compose.yml`
- `appsettings.Docker.json`

Dentro de Docker, la API usa nombres de servicio:

- SQL Server: `sqlserver`

Variables principales:

```text
ASPNETCORE_ENVIRONMENT=Docker
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__SqlServer=Server=sqlserver,1433;Database=WallyBallDb;...
```

## Endpoints Base

- `GET /swagger`: Swagger UI.
- `GET /swagger/v1/swagger.json`: documento Swagger JSON.
- `GET /openapi/v1.json`: documento OpenAPI nativo.
- `GET /api/v1/campeonatos`: lista campeonatos.
- `POST /api/v1/campeonatos`: crea campeonato.
- `GET /api/v1/categorias`: lista categorias.
- `POST /api/v1/categorias`: crea categoria de catalogo.
- `POST /api/v1/campeonatos/{campeonatoId}/categorias`: asocia categoria a campeonato.
- `GET /api/v1/equipos`: lista equipos.
- `POST /api/v1/equipos`: crea equipo.

## Comandos Utiles

Restaurar paquetes:

```powershell
dotnet restore .\wally-service\WallyBallBackend.csproj
```

Compilar:

```powershell
dotnet build .\wally-service\WallyBallBackend.csproj
```

Ejecutar:

```powershell
dotnet run --project .\wally-service\WallyBallBackend.csproj --urls http://localhost:5167
```

Ejecutar con Docker:

```powershell
docker compose up --build
```
