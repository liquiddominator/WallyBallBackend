# Configuracion de reportes-service

## Ejecutar local

```powershell
dotnet restore .\reportes-service\reportes-service.csproj
dotnet build .\reportes-service\reportes-service.csproj
dotnet run --project .\reportes-service\reportes-service.csproj --urls http://localhost:5187
```

Swagger:

```text
http://localhost:5187/swagger
```

## Docker

```powershell
docker compose up --build
```

Servicios:

- `reportes-api`: API en `http://localhost:5187`.
- `reportes-cassandra`: Cassandra en `localhost:9042`.
- `reportes-cassandra-init`: inicializa keyspace y tablas.

## Proyecciones

Los endpoints de reportes leen Cassandra. Para obtener datos reales, `wally-service` debe publicar o sincronizar sus datos transaccionales hacia las tablas documentadas en:

```text
reportes-service/Docs/database/cassandra/ReportesCassandraScript.cql
```

## Configuracion

- `Jwt`: debe coincidir con `personas-service`.
- `Cassandra`: nodos, puerto y keyspace `wallyball_reportes`.
