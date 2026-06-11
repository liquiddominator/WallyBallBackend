# Guia del Proyecto y Ejecucion

## Descripcion

`wally-service` es una API ASP.NET Core para gestionar el dominio deportivo de campeonatos de wallyball del Deportivo Agape. Contempla administracion de campeonatos, categorias, equipos, jugadores, fixture, resultados, tabla de posiciones, portal de consulta para jugadores y reportes.

La autenticacion, usuarios, roles y refresh tokens pertenecen a `personas-service`.

## Requisitos

- .NET SDK 10 o compatible con `net10.0`.
- SQL Server local o en Docker.
- Apache Cassandra local o en Docker.
- Un JWT valido emitido por `personas-service` para probar endpoints protegidos.
- Un cliente HTTP como navegador, Postman, Insomnia o REST Client.

## Estructura Principal

- `Api`: endpoints HTTP versionados.
- `Application`: casos de uso y validaciones.
- `Domain`: entidades y reglas del negocio deportivo.
- `Infrastructure`: bases de datos, validacion JWT, Cassandra y servicios externos.
- `Docs`: documentacion funcional y tecnica.

Documento de estructura:

```text
wally-service/docs/project-structure.md
```

Catalogo de endpoints:

```text
wally-service/docs/api-endpoints.md
```

## Configuracion Previa

Revisar `wally-service/appsettings.Development.json` y ajustar:

- `ConnectionStrings:SqlServer`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SigningKey`
- `Cassandra:ContactPoints`
- `Cassandra:Keyspace`

La configuracion JWT debe coincidir con la configuracion usada por `personas-service` para emitir tokens.

## Ejecucion Local

Desde la carpeta raiz del servicio:

```powershell
cd .\wally-service
dotnet restore .\wally-service\WallyBallBackend.csproj
dotnet build .\wally-service\WallyBallBackend.csproj
dotnet run --project .\wally-service\WallyBallBackend.csproj --urls http://localhost:5167
```

Abrir Swagger UI:

```text
http://localhost:5167/swagger
```

## Ejecucion con Docker

Desde la carpeta raiz del servicio:

```powershell
cd .\wally-service
docker compose up --build
```

La API queda disponible en:

```text
http://localhost:5167
```

Servicios incluidos:

- `api`: API ASP.NET Core publicada en el puerto local `5167`.
- `sqlserver`: SQL Server 2022 Developer en el puerto local `1433`.
- `cassandra`: Apache Cassandra en el puerto local `9042`.
- `cassandra-init`: crea el keyspace `wallyball` si no existe.

En entorno `Docker`, la API aplica automaticamente las migraciones EF Core de SQL Server al iniciar.

## OpenAPI

En entorno `Development` o `Docker`, Swagger UI queda disponible en:

```text
http://localhost:5167/swagger
```

Swagger UI incluye autorizacion Bearer JWT. Para probar endpoints protegidos, obtener primero un token desde `personas-service`, presionar `Authorize` y pegar solo el token.

## Endpoints Implementados

### Campeonatos

- `GET /api/v1/campeonatos`
- `GET /api/v1/campeonatos/{campeonatoId}`
- `POST /api/v1/campeonatos`
- `PUT /api/v1/campeonatos/{campeonatoId}`
- `PATCH /api/v1/campeonatos/{campeonatoId}/finalizar`

### Categorias

- `GET /api/v1/categorias`
- `GET /api/v1/categorias?campeonatoId={campeonatoId}`
- `GET /api/v1/campeonatos/{campeonatoId}/categorias`
- `GET /api/v1/categorias/{categoriaId}`
- `POST /api/v1/categorias`
- `POST /api/v1/campeonatos/{campeonatoId}/categorias`

Crear categoria de catalogo:

```http
POST /api/v1/categorias
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "nombre": "Infantil"
}
```

Asociar categoria a campeonato:

```http
POST /api/v1/campeonatos/1/categorias
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "idCategoria": 1
}
```

### Equipos

- `GET /api/v1/equipos`
- `GET /api/v1/equipos?campeonatoCategoriaId={campeonatoCategoriaId}`
- `GET /api/v1/campeonatos-categorias/{campeonatoCategoriaId}/equipos`
- `GET /api/v1/equipos/{equipoId}`
- `POST /api/v1/equipos`
- `PUT /api/v1/equipos/{equipoId}`

Crear equipo:

```http
POST /api/v1/equipos
Authorization: Bearer {accessToken}
IdCampeonatoCategoria: 1
Content-Type: application/json
```

```json
{
  "nombre": "Halcones"
}
```

### Jugadores

- `GET /api/v1/jugadores`
- `GET /api/v1/jugadores?termino={termino}`
- `GET /api/v1/jugadores?cedula={cedula}`
- `GET /api/v1/jugadores?equipoId={equipoId}`
- `GET /api/v1/jugadores/{jugadorId}`
- `POST /api/v1/jugadores`
- `GET /api/v1/equipos/{equipoId}/jugadores`
- `POST /api/v1/equipos/{equipoId}/jugadores`

Crear jugador:

```http
POST /api/v1/jugadores
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "cedula": "1234567",
  "nombre": "Carlos",
  "apellido": "Perez",
  "telefono": "70000000",
  "fechaNacimiento": "2001-05-10"
}
```

Asignar jugador a equipo:

```http
POST /api/v1/equipos/1/jugadores
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "idJugador": 1
}
```

## Base de Datos SQL Server

SQL Server es la fuente transaccional principal del dominio deportivo.

Script SQL documentado:

```text
wally-service/docs/database/sqlserver/WallyBallDbScript.txt
```

Documentacion de la base de datos:

```text
wally-service/docs/database/sqlserver/database.md
```

Restaurar herramientas:

```powershell
dotnet tool restore
```

Crear una migracion:

```powershell
dotnet tool run dotnet-ef migrations add NombreMigracion `
  --project .\wally-service\WallyBallBackend.csproj `
  --startup-project .\wally-service\WallyBallBackend.csproj `
  --context AppDbContext `
  --output-dir Infrastructure\Persistence\SqlServer\Migrations
```

Aplicar migraciones:

```powershell
dotnet tool run dotnet-ef database update `
  --project .\wally-service\WallyBallBackend.csproj `
  --startup-project .\wally-service\WallyBallBackend.csproj `
  --context AppDbContext
```

## Cassandra

Cassandra se usa para consultas optimizadas por categoria, equipo y jugador:

- `fixture_by_categoria`
- `fixture_by_equipo`
- `fixture_by_jugador`
- `resultados_by_categoria`
- `posiciones_by_categoria`

Script CQL documentado:

```text
wally-service/docs/database/cassandra/WallyBallCassandraScript.txt
```

## Estado Actual

El entorno incluye:

- Configuracion local y Docker.
- Capas base de arquitectura.
- Entidades principales del dominio deportivo.
- `AppDbContext` para SQL Server.
- Migraciones EF Core.
- Factory de sesion para Cassandra.
- Validacion JWT configurada.
- Swagger UI con autorizacion Bearer JWT.
- Epicas 2 a 10 implementadas: campeonatos, categorias, equipos, jugadores, fixture, resultados, posiciones, portal del jugador y reportes.
- Generacion de datos realistas con Bogus para ambientes de desarrollo y Docker.

## Siguientes Pasos Recomendados

- Agregar pruebas unitarias para reglas de fixture y posiciones.
