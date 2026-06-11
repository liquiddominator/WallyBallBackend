# Personas Service - Contexto Funcional

`personas-service` es el microservicio responsable de personas, autenticacion, autorizacion y gestion administrativa base de accesos para el sistema WallyBall.

## Objetivo

Centralizar el registro de usuarios, inicio de sesion, emision de JWT, refresh tokens, roles y consulta de sesion. Los servicios de dominio, como `wally-service`, deben validar tokens emitidos por este servicio y dejar de almacenar credenciales.

## Roles Iniciales

- `ORGANIZADOR`: administra campeonatos, categorias, equipos, jugadores, fixture, resultados y reportes desde los servicios de dominio.
- `JUGADOR`: consulta informacion propia o de su equipo en los servicios de dominio.

## Alcance

- Registro directo de organizadores.
- Inicio de sesion con correo y contrasena.
- Emision de JWT.
- Emision, almacenamiento como hash, rotacion y revocacion de refresh tokens.
- Cierre de sesion.
- Consulta de sesion actual.
- Cambio de contrasena.
- Bloqueo temporal por intentos fallidos.
- Rate limiting para endpoints de autenticacion.
- Consulta administrativa de roles y usuarios.

## Epicas e Historias

### Epica 1: Autenticacion y Acceso

#### HU-A0: Registro de usuario

Como usuario del sistema  
Quiero registrarme con correo, contrasena y nombre opcional  
Para obtener acceso inicial como organizador del sistema.

Criterios de aceptacion:

- El sistema debe solicitar correo, contrasena y nombre opcional.
- El registro directo debe asignar siempre el rol `ORGANIZADOR`.
- El correo debe ser unico.
- La contrasena debe almacenarse con hash seguro.
- Si el registro es correcto, se genera un JWT y un refresh token.
- El registro de usuarios `JUGADOR` sera realizado por un `ORGANIZADOR` en un flujo administrativo posterior.

#### HU-01: Inicio de sesion

Como usuario del sistema  
Quiero iniciar sesion con mi correo y contrasena  
Para acceder a las funcionalidades que me corresponden segun mi rol.

Criterios de aceptacion:

- El sistema debe solicitar correo y contrasena.
- El correo debe existir en el sistema.
- La contrasena debe ser validada.
- Si las credenciales son correctas, se genera un JWT.
- Si las credenciales son correctas, se genera un refresh token.
- Si las credenciales son incorrectas, se muestra un mensaje de error.
- El sistema debe identificar el rol del usuario.
- El sistema debe bloquear temporalmente la cuenta al superar el maximo configurado de intentos fallidos.

#### HU-02: Cerrar sesion

Como usuario autenticado  
Quiero cerrar sesion  
Para finalizar de forma segura mi acceso al sistema.

Criterios de aceptacion:

- Debe requerir JWT valido.
- El backend debe revocar el refresh token recibido.
- El cliente debe retirar el JWT y el refresh token almacenados.
- No debe permitir renovar sesion con un refresh token revocado.

#### HU-A1: Refrescar sesion

Como usuario autenticado  
Quiero renovar mi sesion usando un refresh token  
Para mantener mi acceso sin iniciar sesion nuevamente.

Criterios de aceptacion:

- El sistema debe recibir un refresh token.
- El refresh token debe existir, estar activo y no estar expirado.
- Si el refresh token es valido, se genera un nuevo JWT.
- Si el refresh token es valido, se genera un nuevo refresh token.
- El refresh token anterior debe quedar revocado.
- Si el refresh token es invalido, expirado o revocado, se debe rechazar la solicitud.

#### HU-A2: Consultar sesion actual

Como usuario autenticado  
Quiero consultar mis datos basicos y roles  
Para inicializar correctamente mi sesion en el cliente.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe devolver identificador de usuario, correo, nombre y roles.
- No debe exponer `PasswordHash`.
- No debe exponer refresh tokens.
- Si el token es invalido, debe responder no autorizado.

#### HU-A3: Cambiar contrasena

Como usuario autenticado  
Quiero cambiar mi contrasena  
Para mantener segura mi cuenta.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe solicitar contrasena actual y nueva contrasena.
- La contrasena actual debe ser validada.
- La nueva contrasena debe ser diferente a la actual.
- La nueva contrasena debe almacenarse con hash seguro.
- Al cambiar la contrasena, los refresh tokens activos deben revocarse.

#### HU-A4: Bloqueo por intentos fallidos

Como sistema  
Quiero bloquear temporalmente cuentas con demasiados intentos fallidos  
Para reducir ataques de fuerza bruta.

Criterios de aceptacion:

- El sistema debe contar intentos fallidos de inicio de sesion.
- El contador debe aumentar cuando la contrasena es incorrecta.
- El contador debe reiniciarse cuando el login es correcto.
- Al superar el maximo configurado, la cuenta debe bloquearse temporalmente.
- Una cuenta bloqueada no debe iniciar sesion aunque la contrasena sea correcta.

#### HU-A5: Limite de solicitudes de autenticacion

Como sistema  
Quiero limitar solicitudes a endpoints de autenticacion  
Para reducir abuso y fuerza bruta.

Criterios de aceptacion:

- Los endpoints de autenticacion deben aplicar rate limiting por IP.
- Al superar el limite, el sistema debe responder `429 Too Many Requests`.
- El limite debe aplicarse sin bloquear el resto de la API.

### Epica 1.1: Gestion Administrativa Base

#### HU-G1: Consultar roles

Como organizador  
Quiero consultar los roles disponibles del sistema  
Para conocer las opciones de acceso existentes.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe requerir rol `ORGANIZADOR`.
- Debe mostrar todos los roles registrados.
- Debe mostrar nombre, descripcion y estado del rol.
- No debe permitir crear, editar ni eliminar roles.

#### HU-G2: Consultar rol por id

Como organizador  
Quiero consultar un rol especifico por id  
Para revisar su detalle.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe requerir rol `ORGANIZADOR`.
- Debe devolver el rol solicitado si existe.
- Si el rol no existe, debe responder no encontrado.

#### HU-G3: Consultar usuarios

Como organizador  
Quiero consultar usuarios registrados  
Para administrar accesos del sistema.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe requerir rol `ORGANIZADOR`.
- Debe mostrar usuarios registrados.
- Debe mostrar roles asociados a cada usuario.
- No debe exponer hashes de contrasena.
- No debe exponer refresh tokens.

#### HU-G4: Consultar usuario por id

Como organizador  
Quiero consultar un usuario especifico por id  
Para revisar su detalle de acceso.

Criterios de aceptacion:

- Debe requerir JWT valido.
- Debe requerir rol `ORGANIZADOR`.
- Debe devolver el usuario solicitado si existe.
- Debe incluir roles asociados.
- Si el usuario no existe, debe responder no encontrado.
- No debe exponer hashes de contrasena ni refresh tokens.

## Entidades Base Previstas

- Usuario
- Rol
- UsuarioRol
- RefreshToken

## Reglas de Negocio

- El correo debe ser unico.
- La contrasena debe persistirse solamente como hash seguro.
- El refresh token plano nunca debe almacenarse.
- Cada refresh exitoso debe rotar el token.
- El cambio de contrasena debe revocar refresh tokens activos.
- El login debe bloquear temporalmente una cuenta al superar el limite de intentos fallidos.
- Los servicios de dominio no deben administrar credenciales ni refresh tokens.
