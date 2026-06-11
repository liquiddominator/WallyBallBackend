# Documentacion de Base de Datos Cassandra

Script principal:

```text
wally-service/docs/database/cassandra/WallyBallCassandraScript.txt
```

Keyspace:

```text
wallyball
```

Motor:

```text
Apache Cassandra
```

## Objetivo

Cassandra se usa como modelo de lectura denormalizado para consultas rapidas del sistema de campeonatos de wallyball.

La base transaccional principal es SQL Server. Cassandra no debe ser la fuente de verdad para crear campeonatos, equipos, partidos o resultados. Su funcion es responder consultas de alto rendimiento ya preparadas por caso de uso.

## Responsabilidades

- Consultas rapidas de fixture.
- Consultas rapidas de resultados.
- Consultas rapidas de tabla de posiciones.
- Consultas por equipo.
- Consultas por jugador.
- Lecturas optimizadas para portal del jugador y reportes.

## Principio de diseno

Las tablas Cassandra se disenan por consulta. Esto significa que se duplican datos cuando es necesario para evitar joins, porque Cassandra no esta pensada para resolver relaciones normalizadas como SQL Server.

Reglas practicas:

- Cada tabla responde una consulta concreta.
- No se usan joins.
- No se consulta por columnas que no formen parte de la clave primaria, salvo que se agregue una tabla nueva para esa consulta.
- Los datos se sincronizan desde SQL Server despues de generar fixture, registrar resultados o recalcular posiciones.
- La aplicacion debe asumir consistencia eventual entre SQL Server y Cassandra.

## Keyspace

El keyspace esperado para desarrollo es:

```cql
CREATE KEYSPACE IF NOT EXISTS wallyball
WITH replication = {
  'class': 'SimpleStrategy',
  'replication_factor': 1
};
```

Para produccion se debe evaluar `NetworkTopologyStrategy` y un factor de replicacion acorde a la infraestructura.

## Tablas

### fixture_by_categoria

Consulta que resuelve:

```text
Ver fixture completo de una categoria.
```

Clave primaria:

```cql
PRIMARY KEY ((categoria_id), fecha_partido, partido_id)
```

Diseno:

- `categoria_id` es la partition key.
- `fecha_partido` ordena los partidos dentro de la categoria.
- `partido_id` evita colisiones si dos partidos tienen la misma fecha/hora.

Campos principales:

- `campeonato_id`, `campeonato_nombre`
- `categoria_id`, `categoria_nombre`
- `fase`
- `jornada_numero`
- `partido_id`
- `fecha_partido`
- `equipo_local_id`, `equipo_local`
- `equipo_visitante_id`, `equipo_visitante`
- `estado`

Uso esperado:

```cql
SELECT *
FROM fixture_by_categoria
WHERE categoria_id = ?;
```

### fixture_by_equipo

Consulta que resuelve:

```text
Ver partidos de un equipo.
```

Clave primaria:

```cql
PRIMARY KEY ((equipo_id), fecha_partido, partido_id)
```

Diseno:

- `equipo_id` permite consultar el calendario de un equipo sin filtrar toda la categoria.
- Se guarda `rival_id`, `rival_nombre` y `condicion` para mostrar si el equipo juega como local o visitante.

Uso esperado:

```cql
SELECT *
FROM fixture_by_equipo
WHERE equipo_id = ?;
```

### fixture_by_jugador

Consulta que resuelve:

```text
Ver fixture personal del jugador.
```

Clave primaria:

```cql
PRIMARY KEY ((jugador_id), fecha_partido, partido_id)
```

Diseno:

- `jugador_id` es la partition key para resolver directamente el portal del jugador.
- La tabla duplica datos del equipo y partido para no requerir joins.

Uso esperado:

```cql
SELECT *
FROM fixture_by_jugador
WHERE jugador_id = ?;
```

### resultados_by_categoria

Consulta que resuelve:

```text
Ver resultados de una categoria.
```

Clave primaria:

```cql
PRIMARY KEY ((categoria_id), fecha_resultado, partido_id)
```

Orden:

```cql
WITH CLUSTERING ORDER BY (fecha_resultado DESC, partido_id ASC)
```

Diseno:

- `categoria_id` agrupa resultados por categoria.
- `fecha_resultado` permite mostrar resultados recientes primero.
- Se guardan equipos, sets y ganador como datos denormalizados.

Uso esperado:

```cql
SELECT *
FROM resultados_by_categoria
WHERE categoria_id = ?;
```

### posiciones_by_categoria

Consulta que resuelve:

```text
Ver tabla de posiciones de una categoria.
```

Clave primaria:

```cql
PRIMARY KEY ((categoria_id), posicion, equipo_id)
```

Diseno:

- `categoria_id` agrupa la tabla.
- `posicion` ya viene calculada desde SQL Server o desde el servicio de sincronizacion.
- `equipo_id` evita colisiones y mantiene unicidad dentro de una posicion.

Campos principales:

- `posicion`
- `equipo_id`, `equipo_nombre`
- `partidos_jugados`
- `ganados`
- `perdidos`
- `sets_favor`
- `sets_contra`
- `diferencia_sets`
- `puntos_favor`
- `puntos_contra`
- `diferencia_puntos`
- `puntos`
- `fecha_actualizacion`

Uso esperado:

```cql
SELECT *
FROM posiciones_by_categoria
WHERE categoria_id = ?;
```

## Sincronizacion desde SQL Server

SQL Server mantiene la logica transaccional:

- Generacion de fixture.
- Registro de resultados.
- Recalculo de posiciones.
- Auditoria de cambios.
- Validaciones de integridad.

Cassandra debe actualizarse despues de esas operaciones. Flujo recomendado:

1. La API ejecuta el caso de uso contra SQL Server.
2. SQL Server confirma la transaccion.
3. La aplicacion publica o ejecuta una sincronizacion hacia Cassandra.
4. Cassandra guarda las proyecciones de lectura.

En una primera version, la sincronizacion puede hacerse desde el mismo caso de uso de aplicacion despues del commit SQL. En una version posterior, puede moverse a eventos de dominio o una cola.

## Operaciones de escritura recomendadas

### Al generar fixture

Actualizar:

- `fixture_by_categoria`
- `fixture_by_equipo`
- `fixture_by_jugador`

### Al reprogramar partido

Actualizar o reinsertar:

- `fixture_by_categoria`
- `fixture_by_equipo`
- `fixture_by_jugador`

Nota:

Si cambia `fecha_partido`, cambia parte de la clave primaria. En Cassandra se debe eliminar la fila anterior e insertar la nueva.

### Al registrar resultado

Actualizar:

- `resultados_by_categoria`
- `posiciones_by_categoria`

Opcionalmente actualizar estado del partido en:

- `fixture_by_categoria`
- `fixture_by_equipo`
- `fixture_by_jugador`

### Al modificar resultado

Actualizar:

- `resultados_by_categoria`
- `posiciones_by_categoria`

Si `fecha_resultado` cambia, eliminar la fila anterior e insertar la nueva.

## Consideraciones importantes

- Cassandra no reemplaza las relaciones ni constraints de SQL Server.
- Las tablas duplican nombres de campeonato, categoria, equipo y fase para responder rapido.
- No se debe hacer `ALLOW FILTERING` en consultas normales.
- Si aparece una nueva consulta importante, crear una nueva tabla orientada a esa consulta.
- Si una columna forma parte de la clave primaria y cambia, se debe borrar la fila vieja e insertar la nueva.
- La tabla de posiciones debe escribirse ya ordenada usando `posicion`.

## Relacion con casos de uso

- CU-05 Generar Fixture: alimenta `fixture_by_categoria`, `fixture_by_equipo`, `fixture_by_jugador`.
- CU-06 Registrar Resultado: alimenta `resultados_by_categoria` y refresca `posiciones_by_categoria`.
- CU-07 Consultar Fixture: lee `fixture_by_categoria` o `fixture_by_equipo`.
- CU-08 Consultar Posiciones: lee `posiciones_by_categoria`.
- HU-21 Consultar fixture personal: lee `fixture_by_jugador`.
- HU-22 Consultar resultados: lee `resultados_by_categoria`.
- HU-23 Consultar posiciones: lee `posiciones_by_categoria`.
