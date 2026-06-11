# Endpoints Wally Service

Este documento concentra los endpoints HTTP de `wally-service`. El contexto funcional, epicas e historias viven en `project-context.md`.

## Convenciones

- Base local: `http://localhost:5167`.
- Version actual: `v1`.
- Todas las rutas operativas requieren JWT emitido por `personas-service`.
- Los endpoints implementados actualmente requieren rol `ORGANIZADOR`.
- En Swagger se debe pegar solo el JWT, sin el prefijo `Bearer`.

Header comun:

```http
Authorization: Bearer {accessToken}
```

## Campeonatos

### Listar campeonatos

```http
GET /api/v1/campeonatos
```

Respuesta: lista de campeonatos.

### Obtener campeonato por id

```http
GET /api/v1/campeonatos/{campeonatoId}
```

Respuesta: campeonato solicitado o `404`.

### Crear campeonato

```http
POST /api/v1/campeonatos
Content-Type: application/json
```

Body:

```json
{
  "nombre": "Campeonato Apertura 2026",
  "fechaInicio": "2026-07-01",
  "fechaFin": "2026-08-31"
}
```

Respuestas relevantes: `201`, `400`, `409`.

### Editar campeonato

```http
PUT /api/v1/campeonatos/{campeonatoId}
Content-Type: application/json
```

Body:

```json
{
  "nombre": "Campeonato Apertura 2026 Actualizado",
  "fechaInicio": "2026-07-01",
  "fechaFin": "2026-09-05"
}
```

Respuestas relevantes: `200`, `400`, `404`, `409`.

### Finalizar campeonato

```http
PATCH /api/v1/campeonatos/{campeonatoId}/finalizar
```

Respuestas relevantes: `200`, `404`, `409`.

## Categorias

### Listar categorias

```http
GET /api/v1/categorias
GET /api/v1/categorias?campeonatoId={campeonatoId}
```

Respuesta: lista de categorias del catalogo o filtradas por campeonato.

### Listar categorias de un campeonato

```http
GET /api/v1/campeonatos/{campeonatoId}/categorias
```

Respuesta: categorias asociadas al campeonato.

### Obtener categoria por id

```http
GET /api/v1/categorias/{categoriaId}
```

Respuesta: categoria solicitada o `404`.

### Crear categoria de catalogo

```http
POST /api/v1/categorias
Content-Type: application/json
```

Body:

```json
{
  "nombre": "Infantil"
}
```

Respuestas relevantes: `201`, `400`, `409`.

### Asociar categoria a campeonato

```http
POST /api/v1/campeonatos/{campeonatoId}/categorias
Content-Type: application/json
```

Body:

```json
{
  "idCategoria": 1
}
```

Respuesta: asociacion con `idCampeonatoCategoria`.

Respuestas relevantes: `201`, `400`, `404`, `409`.

## Equipos

### Listar equipos

```http
GET /api/v1/equipos
GET /api/v1/equipos?campeonatoCategoriaId={campeonatoCategoriaId}
```

Respuesta: lista de equipos, incluyendo cantidad de jugadores activos.

### Listar equipos por categoria de campeonato

```http
GET /api/v1/campeonatos-categorias/{campeonatoCategoriaId}/equipos
```

Respuesta: equipos inscritos en la categoria de campeonato.

### Obtener equipo por id

```http
GET /api/v1/equipos/{equipoId}
```

Respuesta: equipo solicitado o `404`.

### Crear equipo

```http
POST /api/v1/equipos
IdCampeonatoCategoria: {campeonatoCategoriaId}
Content-Type: application/json
```

Body:

```json
{
  "nombre": "Halcones"
}
```

Respuestas relevantes: `201`, `400`, `404`, `409`.

### Editar equipo

```http
PUT /api/v1/equipos/{equipoId}
Content-Type: application/json
```

Body:

```json
{
  "nombre": "Halcones Dorados"
}
```

Respuestas relevantes: `200`, `400`, `404`, `409`.

## Jugadores

### Listar y buscar jugadores

```http
GET /api/v1/jugadores
GET /api/v1/jugadores?termino={termino}
GET /api/v1/jugadores?cedula={cedula}
GET /api/v1/jugadores?equipoId={equipoId}
```

Respuesta: lista de jugadores con sus equipos asociados.

### Obtener jugador por id

```http
GET /api/v1/jugadores/{jugadorId}
```

Respuesta: jugador solicitado o `404`.

### Crear jugador

```http
POST /api/v1/jugadores
Content-Type: application/json
```

Body:

```json
{
  "cedula": "1234567",
  "nombre": "Carlos",
  "apellido": "Perez",
  "email": "carlos.perez@example.com",
  "passwordTemporal": "Temporal123!",
  "telefono": "70000000",
  "fechaNacimiento": "2001-05-10"
}
```

Notas:

- `wally-service` solicita a `personas-service` crear la persona y el usuario con rol `JUGADOR`.
- El jugador queda vinculado mediante `idPersona`.
- El organizador realiza un solo registro desde este endpoint.

Respuestas relevantes: `201`, `400`, `409`.

### Listar jugadores de un equipo

```http
GET /api/v1/equipos/{equipoId}/jugadores
```

Respuesta: plantilla activa del equipo.

### Asignar jugador a equipo

```http
POST /api/v1/equipos/{equipoId}/jugadores
Content-Type: application/json
```

Body:

```json
{
  "idJugador": 1
}
```

Respuestas relevantes: `201`, `400`, `404`, `409`.

Conflictos comunes:

- `duplicate_assignment`: el jugador ya pertenece al equipo.
- `team_full`: el equipo ya tiene 12 jugadores activos.
- `inactive_championship`: el campeonato no permite modificaciones.

## Fixture

### Consultar fixture por categoria de campeonato

```http
GET /api/v1/campeonatos-categorias/{campeonatoCategoriaId}/fixture
```

Respuesta: fases, jornadas y partidos de la categoria de campeonato.

Respuestas relevantes: `200`, `404`.

### Generar fixture round-robin

```http
POST /api/v1/campeonatos-categorias/{campeonatoCategoriaId}/fixture/generar
Content-Type: application/json
```

Body:

```json
{
  "fechaPrimeraJornada": "2026-07-06",
  "diasEntreJornadas": 7,
  "horaPartidos": "20:00:00"
}
```

Notas:

- Se genera una fase `ROUND_ROBIN`.
- Se crean jornadas automaticamente.
- Si la cantidad de equipos es impar, se asigna un descanso por jornada.
- No se permite generar un segundo fixture activo para la misma categoria de campeonato.

Respuestas relevantes: `201`, `400`, `404`, `409`.

Conflictos comunes:

- `not_enough_teams`: se requieren al menos dos equipos activos.
- `fixture_already_exists`: ya existe fixture para la categoria de campeonato.
- `inactive_championship`: el campeonato no permite modificaciones.

### Reprogramar partido

```http
PATCH /api/v1/partidos/{partidoId}/reprogramar
Content-Type: application/json
```

Body:

```json
{
  "fechaHoraNueva": "2026-07-08T20:30:00Z",
  "motivo": "Cambio de disponibilidad de cancha"
}
```

Notas:

- Solo cambia fecha y hora.
- No modifica los equipos del partido.
- Registra historial en `ReprogramacionesPartido`.

Respuestas relevantes: `200`, `400`, `404`, `409`.

## Resultados

### Listar resultados

```http
GET /api/v1/resultados
GET /api/v1/resultados?campeonatoCategoriaId={campeonatoCategoriaId}
```

Respuesta: resultados registrados, incluyendo partido, equipos, ganador y detalle de sets.

### Obtener resultado por id

```http
GET /api/v1/resultados/{resultadoId}
```

Respuesta: resultado solicitado o `404`.

### Obtener resultado de un partido

```http
GET /api/v1/partidos/{partidoId}/resultado
```

Respuesta: resultado del partido o `404`.

### Registrar resultado

```http
POST /api/v1/partidos/{partidoId}/resultado
Content-Type: application/json
```

Body:

```json
{
  "sets": [
    {
      "numeroSet": 1,
      "puntosLocal": 25,
      "puntosVisitante": 20
    },
    {
      "numeroSet": 2,
      "puntosLocal": 22,
      "puntosVisitante": 25
    },
    {
      "numeroSet": 3,
      "puntosLocal": 15,
      "puntosVisitante": 12
    }
  ]
}
```

Notas:

- El sistema calcula automaticamente `setsLocal`, `setsVisitante` y equipo ganador.
- El partido queda en estado `FINALIZADO`.
- La tabla de posiciones se recalcula desde SQL Server.
- No se permite registrar un segundo resultado para el mismo partido.

Respuestas relevantes: `201`, `400`, `404`, `409`.

Conflictos comunes:

- `result_already_exists`: el partido ya tiene resultado.
- `inactive_championship`: el campeonato no permite resultados.
- `match_cancelled`: el partido esta cancelado.
- `invalid_result`: el resultado no cumple reglas de integridad.

### Modificar resultado

```http
PUT /api/v1/resultados/{resultadoId}
Content-Type: application/json
```

Body:

```json
{
  "sets": [
    {
      "numeroSet": 1,
      "puntosLocal": 25,
      "puntosVisitante": 18
    },
    {
      "numeroSet": 2,
      "puntosLocal": 25,
      "puntosVisitante": 21
    }
  ],
  "motivo": "Correccion de planilla"
}
```

Notas:

- Reemplaza el detalle de sets del resultado.
- Recalcula ganador y posiciones.
- Registra auditoria del cambio.

Respuestas relevantes: `200`, `400`, `404`, `409`.

### Consultar auditoria de resultado

```http
GET /api/v1/resultados/{resultadoId}/auditoria
```

Respuesta: historial de cambios del resultado.

## Posiciones

### Consultar tabla de posiciones

```http
GET /api/v1/campeonatos-categorias/{campeonatoCategoriaId}/posiciones
```

Respuesta: tabla ordenada de equipos de la categoria de campeonato.

Campos principales:

- `posicion`
- `equipo`
- `puntos`
- `partidosJugados`
- `ganados`
- `perdidos`
- `setsFavor`
- `setsContra`
- `diferenciaSets`
- `puntosFavor`
- `puntosContra`
- `diferenciaPuntos`

Notas:

- La tabla se recalcula automaticamente al registrar o modificar resultados.
- Si una categoria de campeonato todavia no tiene resultados, se devuelven los equipos activos con acumulados en cero.
- El orden usa puntos, ganados, diferencia de sets, diferencia de puntos y nombre del equipo.

Respuestas relevantes: `200`, `404`.

## Datos de prueba

### Generar datos realistas

Disponible solo en ambientes `Development` y `Docker`.

```http
POST /api/v1/dev/datos-prueba
Content-Type: application/json
```

Body:

```json
{
  "categorias": 2,
  "equiposPorCategoria": 4,
  "jugadoresPorEquipo": 8,
  "generarFixture": true,
  "registrarResultados": true,
  "seed": 12345
}
```

Notas:

- Usa Bogus con locale `es`.
- Crea un campeonato activo.
- Crea categorias reutilizables asociadas al campeonato.
- Crea equipos por categoria de campeonato.
- Crea jugadores deportivos con referencias `idPersona` sinteticas para pruebas de dominio.
- Los datos personales reales se crean desde `personas-service` mediante el flujo de alta de jugador.
- Inscribe jugadores en equipos respetando el maximo de 12 jugadores por equipo.
- Opcionalmente genera fixture round-robin.
- Opcionalmente registra resultados para poblar la tabla de posiciones.
- `seed` permite repetir datos similares entre ejecuciones.

Limites:

- `categorias`: 1 a 4.
- `equiposPorCategoria`: 2 a 8.
- `jugadoresPorEquipo`: 1 a 12.

Respuestas relevantes: `201`, `400`, `401`, `403`, `409`.

## Swagger y OpenAPI

```http
GET /swagger
GET /swagger/v1/swagger.json
GET /openapi/v1.json
```
