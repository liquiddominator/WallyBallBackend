using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonasService.Application.Auth;
using PersonasService.Domain.Entities;
using PersonasService.Infrastructure.Persistence.SqlServer;

namespace PersonasService.Infrastructure.Authentication;

public sealed class AuthService : IAuthService
{
    private const string DuplicateEmailCode = "duplicate_email";
    private const string InvalidCredentialsCode = "invalid_credentials";
    private const string InvalidRefreshTokenCode = "invalid_refresh_token";
    private const string InvalidRoleCode = "invalid_role";
    private const string AccountLockedCode = "account_locked";
    private const string OrganizerRoleName = "ORGANIZADOR";

    private readonly IdentityDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IdentityDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var emailExists = await _dbContext.Usuarios
            .AnyAsync(usuario => usuario.Email == email, cancellationToken);

        if (emailExists)
        {
            return AuthResult.Failure(DuplicateEmailCode, "Ya existe un usuario registrado con ese correo.");
        }

        var role = await _dbContext.Roles
            .SingleOrDefaultAsync(rol => rol.Nombre == OrganizerRoleName && rol.Activo, cancellationToken);

        if (role is null)
        {
            return AuthResult.Failure(InvalidRoleCode, "El rol ORGANIZADOR no existe o no esta activo.");
        }

        var user = new Usuario
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            NombreCompleto = NormalizeOptionalText(request.NombreCompleto),
            Activo = true
        };

        user.UsuarioRoles.Add(new UsuarioRol
        {
            Usuario = user,
            Rol = role,
            FechaAsignacion = DateTime.UtcNow
        });

        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(await CreateAuthResponseAsync(user, [role.Nombre], cancellationToken));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.Usuarios
            .Include(usuario => usuario.UsuarioRoles)
            .ThenInclude(usuarioRol => usuarioRol.Rol)
            .SingleOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);

        if (user is null || !user.Activo)
        {
            return AuthResult.Failure(InvalidCredentialsCode, "Correo o contrasena incorrectos.");
        }

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc <= DateTime.UtcNow)
        {
            user.AccessFailedCount = 0;
            user.LockoutEndUtc = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (IsLockedOut(user))
        {
            return AuthResult.Failure(AccountLockedCode, "La cuenta esta bloqueada temporalmente por intentos fallidos.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await RegisterFailedLoginAttemptAsync(user, cancellationToken);
            return AuthResult.Failure(InvalidCredentialsCode, "Correo o contrasena incorrectos.");
        }

        if (user.AccessFailedCount > 0 || user.LockoutEndUtc is not null)
        {
            user.AccessFailedCount = 0;
            user.LockoutEndUtc = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var roles = user.UsuarioRoles
            .Where(usuarioRol => usuarioRol.Rol is not null && usuarioRol.Rol.Activo)
            .Select(usuarioRol => usuarioRol.Rol!.Nombre)
            .Distinct()
            .ToArray();

        return AuthResult.Success(await CreateAuthResponseAsync(user, roles, cancellationToken));
    }

    public async Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.Usuario)
            .ThenInclude(usuario => usuario!.UsuarioRoles)
            .ThenInclude(usuarioRol => usuarioRol.Rol)
            .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null ||
            storedToken.FechaRevocacion is not null ||
            storedToken.FechaExpiracion <= DateTime.UtcNow ||
            storedToken.Usuario is null ||
            !storedToken.Usuario.Activo)
        {
            return AuthResult.Failure(InvalidRefreshTokenCode, "Refresh token invalido o expirado.");
        }

        var roles = storedToken.Usuario.UsuarioRoles
            .Where(usuarioRol => usuarioRol.Rol is not null && usuarioRol.Rol.Activo)
            .Select(usuarioRol => usuarioRol.Rol!.Nombre)
            .Distinct()
            .ToArray();

        var newRawRefreshToken = GenerateRefreshToken();
        var newRefreshToken = CreateStoredRefreshToken(storedToken.Usuario.IdUsuario, newRawRefreshToken);

        storedToken.FechaRevocacion = DateTime.UtcNow;
        storedToken.ReemplazadoPorTokenHash = newRefreshToken.TokenHash;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(CreateAuthResponse(storedToken.Usuario, roles, newRawRefreshToken, newRefreshToken.FechaExpiracion));
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var tokenHash = HashToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.FechaRevocacion is not null)
        {
            return;
        }

        storedToken.FechaRevocacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Usuarios
            .Include(usuario => usuario.UsuarioRoles)
            .ThenInclude(usuarioRol => usuarioRol.Rol)
            .SingleOrDefaultAsync(usuario => usuario.IdUsuario == userId && usuario.Activo, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = user.UsuarioRoles
            .Where(usuarioRol => usuarioRol.Rol is not null && usuarioRol.Rol.Activo)
            .Select(usuarioRol => usuarioRol.Rol!.Nombre)
            .Distinct()
            .ToArray();

        return new AuthUserResponse(user.IdUsuario, user.Email, user.NombreCompleto, roles);
    }

    public async Task<AuthOperationResult> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Usuarios
            .Include(usuario => usuario.RefreshTokens)
            .SingleOrDefaultAsync(usuario => usuario.IdUsuario == userId && usuario.Activo, cancellationToken);

        if (user is null)
        {
            return AuthOperationResult.Failure("user_not_found", "Usuario no encontrado o inactivo.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return AuthOperationResult.Failure(InvalidCredentialsCode, "La contrasena actual es incorrecta.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;

        foreach (var refreshToken in user.RefreshTokens.Where(refreshToken => refreshToken.FechaRevocacion is null))
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthOperationResult.Success();
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        Usuario user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        var rawRefreshToken = GenerateRefreshToken();
        var refreshToken = CreateStoredRefreshToken(user.IdUsuario, rawRefreshToken);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, roles, rawRefreshToken, refreshToken.FechaExpiracion);
    }

    private AuthResponse CreateAuthResponse(
        Usuario user,
        IReadOnlyCollection<string> roles,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.IdPersona.HasValue)
        {
            claims.Add(new Claim("persona_id", user.IdPersona.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.NombreCompleto))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.NombreCompleto));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(
            accessToken,
            expiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            new AuthUserResponse(user.IdUsuario, user.Email, user.NombreCompleto, roles));
    }

    private RefreshToken CreateStoredRefreshToken(int userId, string rawRefreshToken)
    {
        return new RefreshToken
        {
            IdUsuario = userId,
            TokenHash = HashToken(rawRefreshToken),
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        };
    }

    private async Task RegisterFailedLoginAttemptAsync(Usuario user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount += 1;

        if (user.AccessFailedCount >= _jwtOptions.MaxFailedLoginAttempts)
        {
            user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.LockoutMinutes);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsLockedOut(Usuario user)
    {
        return user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow;
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
