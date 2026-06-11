# Estructura del Proyecto

Este documento muestra la estructura general de `wally-service`. El arbol omite archivos generados y detalles internos para mantener una vista limpia.

```text
wally-service/
|-- .dockerignore
|-- docker-compose.yml
|-- dotnet-tools.json
|-- wally-service.slnx
`-- wally-service/
    |-- Api/
    |   |-- Controllers/
    |   `-- OpenApi/
    |-- Application/
    |-- Domain/
    |   |-- Common/
    |   `-- Entities/
    |-- Infrastructure/
    |   |-- Persistence/
    |   |   |-- Cassandra/
    |   |   `-- SqlServer/
    |   |       |-- Migrations/
    |   |       |-- AppDbContext.cs
    |   |       `-- AppDbContextFactory.cs
    |   `-- Authentication/
    |-- Properties/
    |-- Docs/
    |   |-- database/
    |   |   |-- cassandra/
    |   |   |   |-- database.md
    |   |   |   `-- WallyBallCassandraScript.txt
    |   |   `-- sqlserver/
    |   |       |-- database.md
    |   |       `-- WallyBallDbScript.txt
    |   |-- architecture.md
    |   |-- api-endpoints.md
    |   |-- environment.md
    |   |-- project-context.md
    |   |-- project-setup.md
    |   `-- project-structure.md
    |-- appsettings.json
    |-- appsettings.Development.json
    |-- appsettings.Docker.json
    |-- Dockerfile
    |-- Program.cs
    |-- WallyBallBackend.csproj
    `-- WallyBallBackend.http
```

## Archivos Importantes

- `.dockerignore`: define archivos y carpetas excluidos del contexto Docker.
- `docker-compose.yml`: orquesta API, SQL Server, Cassandra e inicializacion de Cassandra para desarrollo standalone.
- `dotnet-tools.json`: manifiesto de herramientas locales, incluyendo `dotnet-ef`.
- `wally-service.slnx`: solucion del servicio.
- `wally-service/Dockerfile`: construccion de la imagen de la API.
- `wally-service/Program.cs`: configuracion principal de servicios, middleware, Swagger, versionado, validacion JWT y logs.
- `wally-service/WallyBallBackend.csproj`: definicion del proyecto y dependencias NuGet.
- `wally-service/appsettings*.json`: configuracion base, local y Docker.
- `wally-service/WallyBallBackend.http`: archivo de pruebas HTTP.
- `Docs/api-endpoints.md`: catalogo de endpoints HTTP implementados.

## Carpetas Principales

- `Api`: endpoints HTTP y capa de entrada.
- `Application`: casos de uso, validaciones y servicios de aplicacion.
- `Domain`: conceptos centrales del negocio deportivo.
- `Infrastructure`: persistencia, validacion JWT y servicios externos.
- `Docs`: documentacion funcional, tecnica y de base de datos.

## Nota de Identidad

La carpeta `Infrastructure/Authentication` se conserva mientras se completa la extraccion de identidad. Su objetivo en `wally-service` debe limitarse a validar JWT emitidos por `identidad-service`; no debe contener emision de tokens, credenciales ni refresh tokens una vez completada la separacion.

## Carpetas Generadas

Estas carpetas pueden aparecer al compilar o abrir el proyecto y no forman parte de la estructura fuente:

- `.vs`
- `bin`
- `obj`
- `logs`
