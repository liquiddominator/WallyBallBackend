# Puesta en Marcha

## Requisitos

- .NET SDK 10.
- Docker Desktop, si se usara el entorno contenerizado.
- SQL Server local o contenedor.
- Un cliente HTTP como navegador, Postman, Insomnia o REST Client.

## Ejecucion Local

Desde la carpeta raiz del servicio:

```powershell
cd .\identidad-service
dotnet restore .\identidad-service\identidad-service.csproj
dotnet build .\identidad-service\identidad-service.csproj
dotnet run --project .\identidad-service\identidad-service.csproj --urls http://localhost:5097
```

Swagger:

```text
http://localhost:5097/swagger
```

Catalogo de endpoints:

```text
identidad-service/docs/api-endpoints.md
```

## Docker

Desde la carpeta raiz del servicio:

```powershell
cd .\identidad-service
docker compose up --build
```

Servicios:

- `identidad-api`: API expuesta en `http://localhost:5097`.
- `identidad-sqlserver`: SQL Server expuesto en `localhost,1434`.

## Base de Datos SQL Server

Script SQL documentado:

```text
identidad-service/docs/database/sqlserver/IdentidadDbScript.txt
```

Documentacion de base de datos:

```text
identidad-service/docs/database/sqlserver/database.md
```

El script crea:

- `Usuarios`
- `Roles`
- `UsuarioRol`
- `RefreshTokens`
- indices y restricciones principales
- roles base `ORGANIZADOR` y `JUGADOR`

## Migraciones EF Core

El DbContext ya esta configurado con las entidades de identidad. Para crear una nueva migracion:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add InitialIdentityCreate `
  --project .\identidad-service\identidad-service.csproj `
  --startup-project .\identidad-service\identidad-service.csproj `
  --context IdentityDbContext `
  --output-dir Infrastructure\Persistence\SqlServer\Migrations
```

Aplicar migraciones:

```powershell
dotnet tool run dotnet-ef database update `
  --project .\identidad-service\identidad-service.csproj `
  --startup-project .\identidad-service\identidad-service.csproj `
  --context IdentityDbContext
```
