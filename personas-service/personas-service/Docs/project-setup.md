# Puesta en Marcha

## Requisitos

- .NET SDK 10.
- Docker Desktop, si se usara el entorno contenerizado.
- SQL Server local o contenedor.
- Un cliente HTTP como navegador, Postman, Insomnia o REST Client.

## Ejecucion Local

Desde la carpeta raiz del servicio:

```powershell
cd .\personas-service
dotnet restore .\personas-service\personas-service.csproj
dotnet build .\personas-service\personas-service.csproj
dotnet run --project .\personas-service\personas-service.csproj --urls http://localhost:5097
```

Swagger:

```text
http://localhost:5097/swagger
```

Catalogo de endpoints:

```text
personas-service/docs/api-endpoints.md
```

## Docker

Desde la carpeta raiz del servicio:

```powershell
cd .\personas-service
docker compose up --build
```

Servicios:

- `personas-api`: API expuesta en `http://localhost:5097`.
- `personas-sqlserver`: SQL Server expuesto en `localhost,1434`.

## Base de Datos SQL Server

Script SQL documentado:

```text
personas-service/docs/database/sqlserver/PersonasDbScript.txt
```

Documentacion de base de datos:

```text
personas-service/docs/database/sqlserver/database.md
```

El script crea:

- `Usuarios`
- `Roles`
- `UsuarioRol`
- `RefreshTokens`
- indices y restricciones principales
- roles base `ORGANIZADOR` y `JUGADOR`

## Migraciones EF Core

El DbContext ya esta configurado con las entidades de personas e identidad. Para crear una nueva migracion:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add InitialIdentityCreate `
  --project .\personas-service\personas-service.csproj `
  --startup-project .\personas-service\personas-service.csproj `
  --context IdentityDbContext `
  --output-dir Infrastructure\Persistence\SqlServer\Migrations
```

Aplicar migraciones:

```powershell
dotnet tool run dotnet-ef database update `
  --project .\personas-service\personas-service.csproj `
  --startup-project .\personas-service\personas-service.csproj `
  --context IdentityDbContext
```
