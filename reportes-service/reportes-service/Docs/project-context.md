# reportes-service

`reportes-service` es el microservicio de lectura para reportes del sistema WallyBall.

## Responsabilidad

- Exponer reportes para organizadores.
- Usar Apache Cassandra como base obligatoria de reportes.
- Mantener los reportes separados del dominio transaccional de `wally-service`.

## Fuera de alcance

- No crea campeonatos, equipos, jugadores, fixture ni resultados.
- No administra personas, usuarios ni credenciales.
- No reemplaza SQL Server como fuente de verdad transaccional.

## Modelo

SQL Server sigue siendo la fuente de verdad del dominio deportivo en `wally-service`.
Cassandra guarda proyecciones denormalizadas para reportes. Por diseño, estas proyecciones tienen consistencia eventual.

## Alimentacion de Proyecciones

`reportes-service` lee exclusivamente desde Cassandra. Las tablas deben alimentarse desde eventos, jobs o endpoints internos disparados por `wally-service` cuando cambien equipos, jugadores, resultados o posiciones.

La primera version deja definido el microservicio, el contrato de lectura y el modelo Cassandra obligatorio para reportes. La integracion de sincronizacion entre `wally-service` y `reportes-service` debe implementarse como siguiente paso.

## Endpoints

Ver `Docs/api-endpoints.md`.
