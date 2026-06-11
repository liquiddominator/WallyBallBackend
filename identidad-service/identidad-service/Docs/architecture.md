# Arquitectura Identidad Service

`identidad-service` es el microservicio de identidad del sistema WallyBall. Centraliza usuarios, roles, autenticacion, refresh tokens y emision de JWT.

## Capas

- `Api`: controladores, contratos HTTP, filtros OpenAPI y versionado.
- `Application`: casos de uso, DTOs, validadores y puertos de aplicacion.
- `Domain`: entidades y reglas puras del dominio de identidad.
- `Infrastructure`: persistencia, JWT, hashing, integraciones externas y configuracion.
- `Docs`: documentacion funcional y tecnica del servicio.

## Modulos Previstos

- Auth: registro, login, refresh, logout, consulta de sesion y cambio de contrasena.
- Gestion: consultas administrativas de roles y usuarios.
- Seguridad: bloqueo temporal por intentos fallidos y rate limiting.
- Tokens: persistencia de refresh tokens como hash y rotacion segura.

## Persistencia

- SQL Server es la fuente transaccional de identidad.
- Base: `IdentidadDb`.
- Script documentado: `identidad-service/docs/database/sqlserver/IdentidadDbScript.txt`.
- El `IdentityDbContext` esta configurado con usuarios, roles, relacion usuario-rol y refresh tokens.

## Versionado y Rutas

Las rutas usan versionado explicito:

```text
/api/v1/{recurso}
```

El catalogo mantenido de endpoints esta en:

```text
Docs/api-endpoints.md
```

## Seguridad

- El servicio emite JWT para clientes y servicios consumidores.
- Los refresh tokens se almacenan solo como hash.
- Los endpoints publicos de auth deberan estar marcados con `[AllowAnonymous]`.
- Los endpoints administrativos deberan requerir rol `ORGANIZADOR` o el rol administrativo que se defina.
- En desarrollo se usa firma simetrica; en produccion se recomienda un mecanismo de secretos o llaves administradas.

## Integracion con Wally Service

- `wally-service` debe validar tokens emitidos por `identidad-service`.
- `wally-service` no debe almacenar contrasenas ni refresh tokens.
- Los claims relevantes para autorizacion entre servicios son usuario, correo y roles.

## Docker

El servicio incluye Dockerfile y docker-compose standalone para desarrollo local. En una etapa posterior convendra crear un compose raiz que levante `wally-service`, `identidad-service` e infraestructura compartida.
