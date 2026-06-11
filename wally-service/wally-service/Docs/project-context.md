# Wally Service - Contexto Funcional

`wally-service` es el microservicio de dominio deportivo del sistema WallyBall para administrar campeonatos, categorias, equipos, jugadores, fixture, resultados, posiciones, portal de consulta y reportes.

La identidad, autenticacion, refresh tokens, usuarios y roles pertenecen a `identidad-service`.

## Objetivo

Construir una API de dominio para administrar campeonatos de wallyball del Deportivo Agape. El servicio valida JWT emitidos por `identidad-service`, pero no administra credenciales ni sesiones.

## Roles Consumidos

- `ORGANIZADOR`: administra campeonatos, categorias, equipos, jugadores, fixture, resultados y reportes.
- `JUGADOR`: consulta fixture personal, resultados y tabla de posiciones.
- `Sistema`: actualiza posiciones automaticamente cuando se registran resultados.

## Alcance Funcional

- Gestion de campeonatos activos y finalizados.
- Gestion de categorias reutilizables.
- Asociacion de categorias a campeonatos.
- Gestion de equipos por categoria inscrita en un campeonato.
- Gestion y busqueda de jugadores.
- Asignacion de jugadores a equipos con limite de 12 jugadores.
- Generacion automatica de fixture por categoria de campeonato con formato todos contra todos.
- Consulta y reprogramacion de partidos.
- Registro y modificacion de resultados.
- Actualizacion automatica de tabla de posiciones.
- Consultas para portal del jugador.
- Reportes de equipos, jugadores, resultados y posiciones.

## Fuera de Alcance

- Registro de usuarios.
- Inicio de sesion.
- Refresh tokens.
- Logout.
- Cambio de contrasena.
- Administracion de roles y credenciales.

Estas responsabilidades viven en `identidad-service`.

## Epicas e Historias de Usuario

### Epica 2: Gestion de Campeonatos

#### HU-03: Crear campeonato

Como organizador  
Quiero registrar campeonatos  
Para administrar torneos deportivos.

Criterios de aceptacion:

- El nombre es obligatorio.
- Debe registrar fecha de inicio.
- Debe registrar fecha de finalizacion.
- Debe registrarse con estado `ACTIVO`.
- Debe almacenarse en base de datos.

#### HU-04: Editar campeonato

Como organizador  
Quiero modificar datos de un campeonato  
Para mantener informacion actualizada.

Criterios de aceptacion:

- Solo campeonatos activos pueden modificarse.
- Debe registrar cambios correctamente.
- Debe validar fechas.

#### HU-05: Finalizar campeonato

Como organizador  
Quiero finalizar un campeonato  
Para impedir nuevas modificaciones.

Criterios de aceptacion:

- El campeonato cambia a estado `FINALIZADO`.
- No se permiten nuevas categorias asociadas.
- No se permiten nuevos equipos.
- No se permiten nuevos resultados.

### Epica 3: Gestion de Categorias

#### HU-06: Registrar categoria

Como organizador  
Quiero crear categorias reutilizables  
Para organizar equipos por nivel.

Criterios de aceptacion:

- El nombre es obligatorio.
- No puede existir una categoria duplicada en el catalogo.
- Una categoria puede asociarse a varios campeonatos.
- Un campeonato puede tener varias categorias.
- No puede repetirse la misma categoria dentro de un campeonato.

Notas:

- La creacion de categoria agrega una categoria al catalogo reutilizable.
- La asociacion de categoria vincula una categoria existente con un campeonato.
- La asociacion devuelve `idCampeonatoCategoria`, identificador operativo usado por equipos, fixture y posiciones.

#### HU-07: Listar categorias

Como organizador  
Quiero visualizar categorias registradas  
Para administrarlas.

Criterios de aceptacion:

- Debe mostrar todas las categorias.
- Debe permitir filtrado por campeonato.

### Epica 4: Gestion de Equipos

#### HU-08: Registrar equipo

Como organizador  
Quiero registrar equipos  
Para que participen en el campeonato.

Criterios de aceptacion:

- El nombre es obligatorio.
- Debe pertenecer a una categoria asociada a un campeonato.
- No debe existir un nombre duplicado dentro de la misma categoria del mismo campeonato.

Notas:

- La categoria del campeonato se envia en el header `IdCampeonatoCategoria`.
- Si la categoria del campeonato no existe, responde `404`.
- Si el campeonato no esta activo o el equipo ya existe en esa categoria del campeonato, responde `409`.

#### HU-09: Editar equipo

Como organizador  
Quiero modificar informacion de un equipo  
Para mantenerla actualizada.

Criterios de aceptacion:

- Debe permitir cambiar nombre.
- Debe validar duplicados.
- Solo permite editar equipos de campeonatos activos.

#### HU-10: Consultar equipos

Como organizador  
Quiero visualizar equipos registrados  
Para administrarlos.

Criterios de aceptacion:

- Debe mostrar equipos por categoria de campeonato.
- Debe mostrar cantidad de jugadores.

### Epica 5: Gestion de Jugadores

#### HU-11: Registrar jugador

Como organizador  
Quiero registrar jugadores  
Para asignarlos a equipos.

Criterios de aceptacion:

- Debe registrar nombre.
- Debe registrar apellido.
- Debe registrar numero de cedula.
- La cedula debe ser unica.

#### HU-12: Asignar jugador a equipo

Como organizador  
Quiero asignar jugadores a equipos  
Para conformar plantillas.

Criterios de aceptacion:

- Debe existir el jugador.
- Debe existir el equipo.
- El equipo no puede superar 12 jugadores.
- No debe asignarse dos veces al mismo equipo.

#### HU-13: Buscar jugador

Como organizador  
Quiero buscar jugadores  
Para encontrarlos rapidamente.

Criterios de aceptacion:

- Debe permitir busqueda por nombre.
- Debe permitir busqueda por cedula.

### Epica 6: Generacion de Fixture

#### HU-14: Generar fixture

Como organizador  
Quiero generar automaticamente el fixture  
Para evitar programacion manual.

Criterios de aceptacion:

- Debe generarse por categoria de campeonato.
- Debe usar formato todos contra todos.
- No debe repetir partidos.
- Un equipo no puede jugar dos veces en la misma jornada.
- Debe crear jornadas automaticamente.

#### HU-15: Consultar fixture

Como organizador  
Quiero visualizar el fixture generado  
Para controlar la programacion.

Criterios de aceptacion:

- Debe mostrar jornadas.
- Debe mostrar fecha.
- Debe mostrar equipos enfrentados.

#### HU-16: Reprogramar partido

Como organizador  
Quiero modificar fechas de partidos  
Para adaptarme a cambios de calendario.

Criterios de aceptacion:

- Debe permitir cambiar fecha.
- No debe modificar los equipos participantes.

### Epica 7: Resultados

#### HU-17: Registrar resultado

Como organizador  
Quiero registrar resultados de partidos  
Para reflejar el desarrollo del torneo.

Criterios de aceptacion:

- Debe existir el partido.
- Debe registrar sets local.
- Debe registrar sets visitante.
- Debe determinar ganador.
- Debe cambiar estado a `FINALIZADO`.

#### HU-18: Modificar resultado

Como organizador  
Quiero corregir resultados registrados  
Para solucionar errores.

Criterios de aceptacion:

- Debe recalcular posiciones.
- Debe guardar auditoria del cambio.

### Epica 8: Tabla de Posiciones

#### HU-19: Actualizar posiciones automaticamente

Como sistema  
Quiero actualizar posiciones al registrar resultados  
Para mantener la clasificacion actualizada.

Criterios de aceptacion:

- Debe actualizar puntos.
- Debe actualizar ganados.
- Debe actualizar perdidos.
- Debe actualizar partidos jugados.

#### HU-20: Consultar posiciones

Como usuario  
Quiero visualizar la tabla de posiciones  
Para conocer la clasificacion.

Criterios de aceptacion:

- Debe mostrar equipos ordenados.
- Debe mostrar puntos.
- Debe mostrar partidos jugados.

### Epica 9: Portal del Jugador

#### HU-21: Consultar fixture personal

Como jugador  
Quiero visualizar mis proximos partidos  
Para conocer mi calendario.

Criterios de aceptacion:

- Debe mostrar solo partidos de su equipo.
- Debe mostrar fecha y hora.

#### HU-22: Consultar resultados

Como jugador  
Quiero visualizar resultados de mi categoria  
Para seguir el campeonato.

Criterios de aceptacion:

- Debe mostrar resultados actualizados.
- Debe mostrar ganador.

#### HU-23: Consultar posiciones

Como jugador  
Quiero visualizar la tabla de posiciones  
Para conocer el rendimiento de mi equipo.

Criterios de aceptacion:

- Debe mostrar clasificacion completa.
- Debe mostrar posicion de su equipo.

### Epica 10: Reportes

#### HU-24: Reporte de equipos

Como organizador  
Quiero obtener reportes de equipos  
Para analizar participacion.

Criterios de aceptacion:

- Debe mostrar equipos por categoria.
- Debe mostrar cantidad de jugadores.

#### HU-25: Reporte de jugadores

Como organizador  
Quiero obtener reportes de jugadores  
Para controlar inscripciones.

Criterios de aceptacion:

- Debe mostrar jugadores por equipo.
- Debe mostrar jugadores por categoria.

#### HU-26: Reporte de resultados

Como organizador  
Quiero visualizar reportes de resultados  
Para analizar el campeonato.

Criterios de aceptacion:

- Debe permitir filtrar por categoria.
- Debe permitir filtrar por fecha.

#### HU-27: Reporte de posiciones

Como organizador  
Quiero generar reportes de posiciones  
Para visualizar el estado actual del torneo.

Criterios de aceptacion:

- Debe mostrar ranking por categoria.
- Debe permitir exportacion futura.

## Entidades Base

- Campeonato
- Categoria
- CampeonatoCategoria
- Equipo
- Jugador
- InscripcionEquipoJugador
- Fase
- Jornada
- Partido
- Resultado
- ResultadoSet
- TablaPosicion
- AuditoriaResultado
- ReprogramacionPartido

## Reglas de Negocio Iniciales

- Un campeonato finalizado no permite nuevas asociaciones de categorias, equipos ni resultados.
- Una categoria no puede repetirse en el catalogo.
- Una categoria no puede asociarse dos veces al mismo campeonato.
- Un equipo no puede repetirse por nombre dentro de la misma categoria del mismo campeonato.
- La cedula del jugador debe ser unica.
- Un equipo no puede superar 12 jugadores activos.
- El fixture se genera por categoria de campeonato.
- El formato principal del fixture es todos contra todos.
- No se deben repetir enfrentamientos.
- Un equipo no puede jugar dos veces en la misma jornada.
- La reprogramacion solo cambia fecha y hora, no cambia equipos.
- El resultado debe determinar ganador.
- La modificacion de resultados debe recalcular posiciones y guardar auditoria.
- La tabla de posiciones se actualiza desde resultados.
