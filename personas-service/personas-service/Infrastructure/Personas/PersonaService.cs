using PersonasService.Application.Personas;
using PersonasService.Domain.Entities;
using PersonasService.Infrastructure.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace PersonasService.Infrastructure.Personas;

public sealed class PersonaService : IPersonaService
{
    private const string JugadorRoleName = "JUGADOR";

    private readonly IdentityDbContext _dbContext;

    public PersonaService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PersonaOperationResult> CreateJugadorAsync(
        CreateJugadorPersonaRequest request,
        CancellationToken cancellationToken)
    {
        var cedula = request.Cedula.Trim();
        var email = NormalizeEmail(request.Email);

        var duplicatePersona = await _dbContext.Personas
            .AnyAsync(persona => persona.Cedula == cedula, cancellationToken);

        if (duplicatePersona)
        {
            return PersonaOperationResult.Failure("duplicate_person", "Ya existe una persona registrada con esa cedula.");
        }

        var duplicateEmail = await _dbContext.Usuarios
            .AnyAsync(usuario => usuario.Email == email, cancellationToken);

        if (duplicateEmail)
        {
            return PersonaOperationResult.Failure("duplicate_email", "Ya existe un usuario registrado con ese correo.");
        }

        var role = await _dbContext.Roles
            .SingleOrDefaultAsync(rol => rol.Nombre == JugadorRoleName && rol.Activo, cancellationToken);

        if (role is null)
        {
            return PersonaOperationResult.Failure("invalid_role", "El rol JUGADOR no existe o no esta activo.");
        }

        var persona = new Persona
        {
            Cedula = cedula,
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Telefono = NormalizeOptionalText(request.Telefono),
            FechaNacimiento = request.FechaNacimiento,
            Activo = true
        };

        var usuario = new Usuario
        {
            Persona = persona,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            NombreCompleto = $"{persona.Nombre} {persona.Apellido}",
            Activo = true
        };

        usuario.UsuarioRoles.Add(new UsuarioRol
        {
            Usuario = usuario,
            Rol = role,
            FechaAsignacion = DateTime.UtcNow
        });

        _dbContext.Personas.Add(persona);
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PersonaOperationResult.Success(new JugadorPersonaResponse(
            persona.IdPersona,
            usuario.IdUsuario,
            persona.Cedula,
            persona.Nombre,
            persona.Apellido,
            persona.Telefono,
            persona.FechaNacimiento,
            usuario.Email,
            [role.Nombre]));
    }

    public async Task<IReadOnlyCollection<PersonaResponse>> GetPersonasAsync(
        IReadOnlyCollection<int> ids,
        string? termino,
        string? cedula,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Personas
            .AsNoTracking()
            .Where(persona => persona.Activo);

        if (ids.Count > 0)
        {
            query = query.Where(persona => ids.Contains(persona.IdPersona));
        }

        var normalizedTerm = NormalizeOptionalText(termino);

        if (normalizedTerm is not null)
        {
            query = query.Where(persona =>
                persona.Nombre.Contains(normalizedTerm)
                || persona.Apellido.Contains(normalizedTerm)
                || (persona.Nombre + " " + persona.Apellido).Contains(normalizedTerm));
        }

        var normalizedCedula = NormalizeOptionalText(cedula);

        if (normalizedCedula is not null)
        {
            query = query.Where(persona => persona.Cedula.Contains(normalizedCedula));
        }

        return await query
            .OrderBy(persona => persona.Apellido)
            .ThenBy(persona => persona.Nombre)
            .ThenBy(persona => persona.IdPersona)
            .Select(persona => new PersonaResponse(
                persona.IdPersona,
                persona.Cedula,
                persona.Nombre,
                persona.Apellido,
                persona.Telefono,
                persona.FechaNacimiento,
                persona.Activo))
            .ToListAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
