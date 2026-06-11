# Endpoints Personas Service

Este documento concentra los endpoints HTTP de `personas-service`. El contexto funcional, epicas e historias viven en `project-context.md`.

## Convenciones

- Base local: `http://localhost:5097`.
- Version actual: `v1`.
- El servicio emite JWT para clientes y para servicios consumidores como `wally-service`.
- En Swagger se debe pegar solo el JWT, sin el prefijo `Bearer`.

Header para endpoints protegidos:

```http
Authorization: Bearer {accessToken}
```

## Autenticacion

### Registrar organizador

Publico.

```http
POST /api/v1/auth/register
Content-Type: application/json
```

Body:

```json
{
  "email": "organizador@example.com",
  "password": "Password123!",
  "nombreCompleto": "Organizador Demo"
}
```

Notas:

- El registro directo asigna siempre el rol `ORGANIZADOR`.
- El registro de usuarios `JUGADOR` queda reservado para un flujo administrativo posterior.

Respuesta: access token, expiracion, refresh token y datos basicos del usuario.

Respuestas relevantes: `201`, `400`, `409`.

### Iniciar sesion

Publico.

```http
POST /api/v1/auth/login
Content-Type: application/json
```

Body:

```json
{
  "email": "organizador@example.com",
  "password": "Password123!"
}
```

Respuesta: access token, expiracion, refresh token y datos basicos del usuario.

Respuestas relevantes: `200`, `400`, `401`, `423`.

### Refrescar sesion

Publico.

```http
POST /api/v1/auth/refresh
Content-Type: application/json
```

Body:

```json
{
  "refreshToken": "{refreshToken}"
}
```

Respuesta: nuevo access token, nuevo refresh token y expiraciones.

Respuestas relevantes: `200`, `400`, `401`.

### Consultar sesion actual

Requiere JWT valido.

```http
GET /api/v1/auth/me
```

Respuesta: identificador de usuario, correo, nombre y roles.

Respuestas relevantes: `200`, `401`.

### Cambiar contrasena

Requiere JWT valido.

```http
POST /api/v1/auth/change-password
Content-Type: application/json
```

Body:

```json
{
  "currentPassword": "Password123!",
  "newPassword": "NewPassword123!"
}
```

Notas:

- Al cambiar la contrasena se revocan los refresh tokens activos.

Respuestas relevantes: `204`, `400`, `401`.

### Cerrar sesion

Requiere JWT valido.

```http
POST /api/v1/auth/logout
Content-Type: application/json
```

Body:

```json
{
  "refreshToken": "{refreshToken}"
}
```

Respuesta: `204`.

## Gestion Administrativa

Todos los endpoints de gestion requieren JWT valido con rol `ORGANIZADOR`.

### Crear persona y usuario jugador

Usado por `wally-service` durante el alta de jugador.

```http
POST /api/v1/personas/jugadores
Content-Type: application/json
Authorization: Bearer {accessTokenOrganizador}
```

Body:

```json
{
  "cedula": "1234567",
  "nombre": "Carlos",
  "apellido": "Perez",
  "telefono": "70000000",
  "fechaNacimiento": "2001-05-10",
  "email": "carlos.perez@example.com",
  "password": "Temporal123!",
}
```

Notas:

- Crea una persona.
- Crea un usuario vinculado a esa persona con rol `JUGADOR`.
- No crea datos deportivos; esos pertenecen a `wally-service`.
- Devuelve `idPersona`, que `wally-service` guarda como referencia externa.

Respuestas relevantes: `201`, `400`, `401`, `403`, `409`.

### Listar roles

```http
GET /api/v1/gestion/roles
```

Respuesta: lista de roles con identificador, nombre, descripcion y estado.

### Obtener rol por id

```http
GET /api/v1/gestion/roles/{roleId}
```

Respuesta: rol solicitado o `404`.

### Listar usuarios

```http
GET /api/v1/gestion/usuarios
```

Respuesta: lista de usuarios con estado, fechas de seguridad y roles asociados.

### Obtener usuario por id

```http
GET /api/v1/gestion/usuarios/{userId}
```

Respuesta: usuario solicitado o `404`.

## Swagger y OpenAPI

```http
GET /swagger
GET /swagger/v1/swagger.json
GET /openapi/v1.json
```
