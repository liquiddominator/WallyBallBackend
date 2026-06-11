# Endpoints de reportes-service

Todos los endpoints requieren JWT valido con rol `ORGANIZADOR`.

## Reporte de equipos

```http
GET /api/v1/reportes/equipos
GET /api/v1/reportes/equipos?campeonatoId={campeonatoId}
GET /api/v1/reportes/equipos?campeonatoCategoriaId={campeonatoCategoriaId}
```

Lee Cassandra desde `reportes_equipos_by_categoria`.

## Reporte de jugadores

```http
GET /api/v1/reportes/jugadores
GET /api/v1/reportes/jugadores?campeonatoId={campeonatoId}
GET /api/v1/reportes/jugadores?campeonatoCategoriaId={campeonatoCategoriaId}
GET /api/v1/reportes/jugadores?equipoId={equipoId}
```

Lee Cassandra desde `reportes_jugadores_by_categoria`.

## Reporte de resultados

```http
GET /api/v1/reportes/resultados
GET /api/v1/reportes/resultados?campeonatoCategoriaId={campeonatoCategoriaId}
GET /api/v1/reportes/resultados?fechaDesde=2026-06-01&fechaHasta=2026-06-30
```

Lee Cassandra desde `reportes_resultados_by_categoria_fecha`.

## Reporte de posiciones

```http
GET /api/v1/reportes/posiciones
GET /api/v1/reportes/posiciones?campeonatoId={campeonatoId}
GET /api/v1/reportes/posiciones?campeonatoCategoriaId={campeonatoCategoriaId}
```

Lee Cassandra desde `reportes_posiciones_by_categoria`.
