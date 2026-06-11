# Endpoints Wally Service

Este documento concentra los endpoints HTTP de `wally-service`. El contexto funcional, epicas e historias viven en `project-context.md`.

## Convenciones

- Base local: `http://localhost:5167`.
- Version actual: `v1`.
- Todas las rutas operativas requieren JWT emitido por `identidad-service`.
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
  "telefono": "70000000",
  "fechaNacimiento": "2001-05-10"
}
```

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

## Swagger y OpenAPI

```http
GET /swagger
GET /swagger/v1/swagger.json
GET /openapi/v1.json
```
