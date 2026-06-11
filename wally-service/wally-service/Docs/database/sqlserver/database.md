# Documentacion de Base de Datos

Script principal:

```text
wally-service/docs/database/sqlserver/WallyBallDbScript.txt
```

Base de datos:

```text
WallyBallDb
```

Motor:

```text
SQL Server
```

## Objetivo

La base de datos almacena la informacion transaccional del dominio deportivo: campeonatos, categorias, asociaciones categoria-campeonato, equipos, jugadores, inscripciones, fixture, resultados, posiciones, auditoria y reprogramaciones.

La identidad pertenece a `identidad-service` y no forma parte de esta base de dominio.

## Decisiones Aplicadas

- Se mantiene `WallyBallDb` como base transaccional del dominio deportivo.
- Se separo identidad hacia `IdentidadDb` en `identidad-service`.
- Las categorias son reutilizables y se relacionan con campeonatos mediante `CampeonatosCategorias`.
- Equipos, fases, partidos y posiciones dependen de `CampeonatosCategorias` para evitar mezclar datos entre campeonatos que reutilizan la misma categoria.
- Se valida que un equipo no juegue mas de una vez en una jornada.
- Se recalculan posiciones desde resultados y sets.
- Se bloquean cambios operativos cuando el campeonato esta `FINALIZADO`.
- Se guardan fechas tecnicas con `SYSUTCDATETIME()`.

## Jugadores

Representa jugadores registrados en el dominio deportivo.

Campos principales:

- `IdJugador`: identificador interno.
- `Cedula`: identificador civil unico.
- `Nombre`, `Apellido`.
- `Telefono`, `FechaNacimiento`.
- `Activo`.
- `FechaCreacion`, `FechaActualizacion`.

Reglas:

- `Cedula` debe ser unica.
- Un jugador puede asignarse a equipos mediante `InscripcionesEquipoJugador`.

## Campeonatos

Representa un torneo deportivo.

Estados:

- `BORRADOR`
- `ACTIVO`
- `FINALIZADO`
- `CANCELADO`

Reglas:

- `FechaFin` no puede ser menor a `FechaInicio`.
- El estado por defecto es `ACTIVO`.
- Cuando esta `FINALIZADO`, se bloquean cambios operativos en asociaciones de categoria, equipos y resultados.

## Categorias

Catalogo reutilizable de categorias deportivas.

Estados:

- `ACTIVA`
- `INACTIVA`

Reglas:

- El nombre no puede repetirse en el catalogo.
- Puede asociarse a multiples campeonatos mediante `CampeonatosCategorias`.

## CampeonatosCategorias

Relacion muchos a muchos entre campeonatos y categorias. Representa una categoria habilitada dentro de un campeonato especifico.

Estados:

- `ACTIVA`
- `INACTIVA`

Reglas:

- Una categoria no puede asociarse dos veces al mismo campeonato.
- Si el campeonato esta `FINALIZADO`, no se pueden crear ni modificar asociaciones.
- Las entidades operativas del torneo cuelgan de esta relacion.

## Equipos

Representa equipos participantes en una categoria de un campeonato.

Reglas:

- Pertenece a una fila de `CampeonatosCategorias`.
- El nombre no puede repetirse dentro de la misma categoria del mismo campeonato.

## InscripcionesEquipoJugador

Relaciona jugadores con equipos.

Estados:

- `ACTIVO`
- `RETIRADO`

Reglas:

- Un jugador no puede duplicarse dentro del mismo equipo.
- Un equipo no puede tener mas de 12 jugadores activos.

## Fixture

### Fases

Define etapas de competencia dentro de una categoria de campeonato.

Tipos:

- `ROUND_ROBIN`
- `ELIMINATORIA`

Estados:

- `PENDIENTE`
- `ACTIVA`
- `FINALIZADA`
- `CANCELADA`

Reglas:

- El orden de fase es unico por categoria de campeonato.

### Jornadas

Agrupa partidos dentro de una fase.

Reglas:

- El numero de jornada debe ser mayor que cero.
- El numero de jornada no puede repetirse dentro de una fase.

### Partidos

Representa enfrentamientos entre dos equipos.

Estados:

- `PROGRAMADO`
- `REPROGRAMADO`
- `FINALIZADO`
- `CANCELADO`

Reglas:

- Equipo local y visitante deben ser distintos.
- Ambos equipos deben pertenecer a la categoria de campeonato del partido.
- La fase debe pertenecer a la categoria de campeonato del partido.
- La jornada debe pertenecer a la fase del partido.
- No puede repetirse el mismo enfrentamiento dentro de una fase aunque cambie local/visitante.
- Un equipo no puede jugar mas de una vez en la misma jornada.

## Resultados

### Resultados

Guarda el resultado consolidado del partido por sets.

Reglas:

- Un partido solo puede tener un resultado.
- Los sets no pueden ser negativos.
- No se permite empate en sets.
- El ganador debe ser local o visitante.
- El ganador debe coincidir con el mayor numero de sets.
- Al registrar resultado, el partido cambia a `FINALIZADO`.
- Al insertar, actualizar o eliminar resultados, se recalcula la tabla de posiciones.

### ResultadoSets

Guarda detalle de puntos por set.

Reglas:

- El numero de set debe ser mayor que cero.
- Los puntos no pueden ser negativos.
- No se permite empate en un set.
- Al insertar, actualizar o eliminar sets, se recalcula la tabla de posiciones.

### AuditoriaResultados

Registra cambios sobre resultados consolidados.

Campos principales:

- Resultado afectado.
- Sets anteriores.
- Ganador anterior.
- Sets nuevos.
- Ganador nuevo.
- Motivo opcional.
- Fecha de cambio.

## Posiciones

### TablaPosiciones

Almacena la clasificacion acumulada por categoria de campeonato y equipo.

Reglas:

- Una posicion es unica por categoria de campeonato y equipo.
- Todos los acumulados deben ser mayores o iguales a cero.
- Se recalcula desde resultados y sets para evitar doble conteo al modificar resultados.

## Reprogramaciones

### ReprogramacionesPartido

Guarda el historial de cambios de fecha/hora de partidos.

Uso esperado:

- Ejecutar `SP_ReprogramarPartido`.
- El procedimiento inserta historial y actualiza `Partidos.FechaHoraProgramada`.
- El partido queda en estado `REPROGRAMADO`.

## Procedimientos Almacenados

- `SP_CrearTablaPosicionesCategoria`: crea filas iniciales de posiciones para equipos activos de una categoria de campeonato.
- `SP_RecalcularTablaPosicionesCategoria`: recalcula desde cero posiciones por categoria de campeonato.
- `SP_ReprogramarPartido`: reprograma un partido de forma transaccional.
- `SP_RegistrarResultado`: inserta resultado consolidado de un partido.

## Triggers

- `TRG_Maximo12JugadoresPorEquipo`: impide mas de 12 jugadores activos por equipo.
- `TRG_CampeonatosCategorias_BloquearCampeonatoFinalizado`: bloquea asociaciones si el campeonato esta finalizado.
- `TRG_Equipos_BloquearCampeonatoFinalizado`: bloquea equipos si el campeonato esta finalizado.
- `TRG_Partidos_ValidarIntegridad`: valida equipos, fase, jornada y equipo unico por jornada.
- `TRG_Resultados_BloquearCampeonatoFinalizado`: bloquea resultados de campeonato finalizado y valida ganador.
- `TRG_Resultados_Auditoria_Update`: registra auditoria ante cambios de resultado.
- `TRG_Resultados_RecalcularPosiciones`: recalcula posiciones ante cambios en resultados.
- `TRG_ResultadoSets_RecalcularPosiciones`: recalcula posiciones ante cambios en detalle de sets.

## Vistas

- `VW_TablaPosicionesOrdenada`: posiciones con campeonato, categoria, equipo y diferencias.
- `VW_FixtureCompleto`: fixture con campeonato, categoria, fase, jornada, equipos y estado.
- `VW_ResultadosCompletos`: resultados con campeonato, categoria, fase, jornada, equipos, sets y ganador.

## Indices y Restricciones Relevantes

- `UQ_Jugadores_Cedula`: cedula unica.
- `UQ_Categorias_Nombre`: evita categorias duplicadas en el catalogo.
- `UQ_CampeonatosCategorias_Campeonato_Categoria`: evita asociar dos veces la misma categoria al mismo campeonato.
- `UQ_Equipos_CampeonatoCategoria_Nombre`: evita equipos duplicados por categoria del campeonato.
- `UQ_Inscripciones_Equipo_Jugador`: evita asignacion duplicada al mismo equipo.
- `UQ_Fases_CampeonatoCategoria_Orden`: evita dos fases con el mismo orden por categoria del campeonato.
- `UQ_Jornadas_Fase_Numero`: evita jornadas duplicadas en una fase.
- `UQ_Partidos_Fase_Enfrentamiento`: evita repetir enfrentamientos en una fase.
- `UQ_Resultados_Partido`: evita mas de un resultado por partido.
- `UQ_TablaPosiciones_CampeonatoCategoria_Equipo`: evita duplicar posiciones por categoria del campeonato/equipo.

## Ubicacion

La documentacion y el script SQL Server viven en:

```text
wally-service/docs/database/sqlserver
```
