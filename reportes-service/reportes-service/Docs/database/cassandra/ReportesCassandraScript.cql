CREATE KEYSPACE IF NOT EXISTS wallyball_reportes
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

USE wallyball_reportes;

CREATE TABLE IF NOT EXISTS reportes_equipos_by_categoria (
  id_campeonato_categoria int,
  id_equipo int,
  id_campeonato int,
  campeonato text,
  id_categoria int,
  categoria text,
  equipo text,
  activo boolean,
  cantidad_jugadores_activos int,
  fecha_creacion timestamp,
  PRIMARY KEY ((id_campeonato_categoria), equipo, id_equipo)
);

CREATE INDEX IF NOT EXISTS idx_reportes_equipos_campeonato
ON reportes_equipos_by_categoria (id_campeonato);

CREATE TABLE IF NOT EXISTS reportes_jugadores_by_categoria (
  id_campeonato_categoria int,
  id_equipo int,
  id_jugador int,
  id_campeonato int,
  campeonato text,
  id_categoria int,
  categoria text,
  equipo text,
  id_persona int,
  cedula text,
  nombre text,
  apellido text,
  telefono text,
  fecha_nacimiento date,
  estado_inscripcion text,
  fecha_inscripcion timestamp,
  PRIMARY KEY ((id_campeonato_categoria), id_equipo, apellido, nombre, id_jugador)
);

CREATE INDEX IF NOT EXISTS idx_reportes_jugadores_campeonato
ON reportes_jugadores_by_categoria (id_campeonato);

CREATE INDEX IF NOT EXISTS idx_reportes_jugadores_equipo
ON reportes_jugadores_by_categoria (id_equipo);

CREATE TABLE IF NOT EXISTS reportes_resultados_by_categoria_fecha (
  id_campeonato_categoria int,
  fecha_registro date,
  id_resultado int,
  id_partido int,
  id_campeonato int,
  campeonato text,
  id_categoria int,
  categoria text,
  id_fase int,
  fase text,
  id_jornada int,
  numero_jornada int,
  id_equipo_local int,
  equipo_local text,
  id_equipo_visitante int,
  equipo_visitante text,
  sets_local int,
  sets_visitante int,
  id_equipo_ganador int,
  equipo_ganador text,
  fecha_actualizacion timestamp,
  sets_json text,
  PRIMARY KEY ((id_campeonato_categoria), fecha_registro, id_resultado)
) WITH CLUSTERING ORDER BY (fecha_registro DESC, id_resultado DESC);

CREATE TABLE IF NOT EXISTS reportes_posiciones_by_categoria (
  id_campeonato_categoria int,
  posicion int,
  id_campeonato int,
  campeonato text,
  id_categoria int,
  categoria text,
  id_equipo int,
  equipo text,
  partidos_jugados int,
  ganados int,
  perdidos int,
  sets_favor int,
  sets_contra int,
  puntos_favor int,
  puntos_contra int,
  puntos int,
  fecha_actualizacion timestamp,
  PRIMARY KEY ((id_campeonato_categoria), posicion, id_equipo)
);

CREATE INDEX IF NOT EXISTS idx_reportes_posiciones_campeonato
ON reportes_posiciones_by_categoria (id_campeonato);
