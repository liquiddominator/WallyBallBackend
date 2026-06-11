# Documentacion de Base de Datos

Script principal:

```text
identidad-service/docs/database/sqlserver/IdentidadDbScript.txt
```

Base de datos:

```text
IdentidadDb
```

Motor:

```text
SQL Server
```

## Objetivo

La base de datos almacena la informacion transaccional de identidad del sistema WallyBall: usuarios, roles, relacion usuario-rol, refresh tokens, bloqueo temporal y auditoria tecnica basica.

## Usuarios

Almacena credenciales y datos base de acceso.

Campos principales:

- `IdUsuario`: identificador interno.
- `Email`: correo unico para login.
- `PasswordHash`: hash de contrasena.
- `NombreCompleto`: nombre visible del usuario.
- `Activo`: habilita o deshabilita acceso.
- `AccessFailedCount`: intentos fallidos acumulados.
- `LockoutEndUtc`: fecha UTC de fin del bloqueo temporal.
- `PasswordChangedAtUtc`: fecha UTC del ultimo cambio de contrasena.
- `FechaCreacion`, `FechaActualizacion`: auditoria tecnica.

Reglas:

- `Email` debe ser unico.
- La contrasena se almacena como hash desde la aplicacion.
- El login incrementa intentos fallidos y bloquea temporalmente la cuenta segun configuracion.

## Roles

Define roles funcionales del sistema.

Roles iniciales:

- `ORGANIZADOR`
- `JUGADOR`

Reglas:

- El nombre de rol debe ser unico.
- Los roles iniciales se insertan como datos base.

## UsuarioRol

Relacion muchos a muchos entre usuarios y roles.

Reglas:

- Un usuario no puede tener el mismo rol duplicado.
- La asignacion guarda fecha UTC.

## RefreshTokens

Almacena refresh tokens persistidos como hash para renovacion de sesion.

Campos principales:

- `IdRefreshToken`: identificador interno.
- `IdUsuario`: usuario propietario.
- `TokenHash`: hash SHA-256 del refresh token.
- `FechaExpiracion`: fecha limite de uso.
- `FechaRevocacion`: indica si fue revocado por logout, rotacion o cambio de contrasena.
- `ReemplazadoPorTokenHash`: hash del refresh token nuevo cuando hay rotacion.

Reglas:

- `TokenHash` debe ser unico.
- El refresh token plano nunca se almacena.
- Cada refresh exitoso rota el token: revoca el anterior y emite uno nuevo.
- El cambio de contrasena revoca refresh tokens activos.

## Indices y Restricciones Relevantes

- `UQ_Usuarios_Email`: evita correos duplicados.
- `UQ_Roles_Nombre`: evita roles duplicados por nombre.
- `UQ_UsuarioRol_Usuario_Rol`: evita asignar dos veces el mismo rol al mismo usuario.
- `UQ_RefreshTokens_TokenHash`: evita duplicar refresh tokens.
- `IX_RefreshTokens_Usuario_Expiracion`: optimiza consultas de tokens por usuario y expiracion.

## Integracion

`identidad-service` es la fuente de verdad de credenciales y roles. `wally-service` debe consumir JWT emitidos por este servicio y no debe tener tablas de usuarios, roles ni refresh tokens en su base transaccional de dominio.
