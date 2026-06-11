# Arquitectura Wally Service

`wally-service` es el microservicio de dominio deportivo. Expone endpoints para campeonatos, categorias, equipos, jugadores, fixture, resultados, posiciones, portal del jugador y reportes.

La identidad vive en `identidad-service`; este servicio solo valida tokens JWT emitidos por identidad.

## Capas

- `Api`: controladores, contratos HTTP, versionado de rutas y respuestas.
- `Application`: casos de uso, validaciones, DTOs, puertos y servicios de aplicacion.
- `Domain`: entidades y reglas de negocio del campeonato.
- `Infrastructure`: EF Core, SQL Server, Cassandra, validacion JWT, servicios externos y configuracion.
- `Docs`: documentacion funcional y tecnica.

## Modulos Funcionales

- Campeonatos: crear, editar, finalizar y consultar campeonatos.
- Categorias: registrar categorias reutilizables y asociarlas a campeonatos.
- Equipos: registrar, editar y consultar equipos por categoria inscrita en un campeonato.
- Jugadores: registrar, asignar a equipo y buscar.
- Fixture: generar, consultar y reprogramar partidos.
- Resultados: registrar, modificar y auditar resultados.
- Posiciones: actualizar automaticamente y consultar tablas.
- Portal del jugador: consultar fixture personal, resultados y posiciones.
- Reportes: equipos, jugadores, resultados y posiciones.

## Persistencia

- SQL Server: fuente transaccional principal mediante EF Core. Contiene entidades normalizadas del dominio deportivo.
- Cassandra: almacenamiento orientado a consultas denormalizadas para lecturas rapidas por categoria, equipo o jugador.

Script relacional planificado:

```text
wally-service/docs/database/sqlserver/WallyBallDbScript.txt
```

Script Cassandra planificado:

```text
wally-service/docs/database/cassandra/WallyBallCassandraScript.txt
```

## Contenedores

El entorno Docker standalone de este servicio incluye:

- `api`: API ASP.NET Core expuesta en `http://localhost:5167`.
- `sqlserver`: base relacional de dominio `WallyBallDb`.
- `cassandra`: base NoSQL para consultas denormalizadas.
- `cassandra-init`: inicializador del keyspace `wallyball`.

En una orquestacion de microservicios, este servicio debe convivir con `identidad-service` y validar los JWT emitidos por ese servicio.

## Versionado y Rutas

Las rutas usan versionado explicito:

```text
/api/v1/{recurso}
```

Documentacion interactiva disponible en `Development` y `Docker`:

```text
GET /swagger
GET /swagger/v1/swagger.json
GET /openapi/v1.json
```

El catalogo mantenido de endpoints esta en:

```text
Docs/api-endpoints.md
```

## Seguridad

- `wally-service` no registra usuarios ni emite tokens.
- Los endpoints protegidos validan JWT emitidos por `identidad-service`.
- Los endpoints operativos del organizador requieren rol `ORGANIZADOR`.
- Las consultas del jugador deben limitarse a informacion de su equipo/categoria cuando aplique.

## Reglas Transaccionales

- Las operaciones que modifican resultados deben recalcular posiciones en la misma unidad de trabajo.
- Las modificaciones de resultados deben registrar auditoria.
- Finalizar un campeonato bloquea cambios operativos posteriores.
- La generacion de fixture debe validar categoria de campeonato, equipos suficientes y ausencia de fixture previo activo.

## Convenciones Tecnicas

- Identificadores `int` con identity en SQL Server.
- Fechas de negocio con `DateOnly` cuando no se requiere hora.
- Fechas con hora en UTC usando `DateTime`.
- Validaciones de entrada con FluentValidation.
- Logging estructurado con Serilog.
- Configuracion sensible fuera de `appsettings.json` en produccion.
