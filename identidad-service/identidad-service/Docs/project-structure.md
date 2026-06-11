# Estructura

```text
identidad-service/
|-- .dockerignore
|-- docker-compose.yml
|-- identidad-service.slnx
`-- identidad-service/
    |-- Api/
    |   |-- Controllers/
    |   `-- OpenApi/
    |-- Application/
    |-- Domain/
    |-- Docs/
    |   |-- api-endpoints.md
    |   `-- database/
    |       `-- sqlserver/
    |           |-- database.md
    |           `-- IdentidadDbScript.txt
    |-- Infrastructure/
    |   |-- Authentication/
    |   `-- Persistence/
    |       `-- SqlServer/
    |-- Properties/
    |-- appsettings.json
    |-- appsettings.Development.json
    |-- appsettings.Docker.json
    |-- Dockerfile
    |-- identidad-service.csproj
    |-- identidad-service.http
    `-- Program.cs
```

## Archivos principales

- `Program.cs`: configuracion de servicios, middleware, Swagger, versionado, rate limiting, autenticacion y logs.
- `Infrastructure/Persistence/SqlServer/IdentityDbContext.cs`: DbContext de usuarios, roles y refresh tokens.
- `Infrastructure/Authentication/JwtOptions.cs`: configuracion fuertemente tipada para JWT.
- `Docs/database/sqlserver/IdentidadDbScript.txt`: script SQL Server planificado para usuarios, roles y refresh tokens.
- `Docs/api-endpoints.md`: catalogo de endpoints HTTP implementados.
- `Dockerfile`: construccion de la imagen del servicio.
- `docker-compose.yml`: entorno standalone con API y SQL Server.
