using Microsoft.EntityFrameworkCore;
using PersonasService.Domain.Entities;

namespace PersonasService.Infrastructure.Persistence.SqlServer;

public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Persona> Personas => Set<Persona>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsuarios(modelBuilder);
        ConfigureRoles(modelBuilder);
    }

    private static void ConfigureUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Persona>(builder =>
        {
            builder.ToTable("Personas");
            builder.HasKey(persona => persona.IdPersona);
            builder.Property(persona => persona.Cedula).HasMaxLength(30).IsRequired();
            builder.Property(persona => persona.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(persona => persona.Apellido).HasMaxLength(100).IsRequired();
            builder.Property(persona => persona.Telefono).HasMaxLength(30);
            builder.Property(persona => persona.Activo).HasDefaultValue(true);
            builder.Property(persona => persona.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(persona => persona.Cedula)
                .IsUnique()
                .HasDatabaseName("UQ_Personas_Cedula");
        });

        modelBuilder.Entity<Usuario>(builder =>
        {
            builder.ToTable("Usuarios");
            builder.HasKey(usuario => usuario.IdUsuario);
            builder.Property(usuario => usuario.Email).HasMaxLength(150).IsRequired();
            builder.Property(usuario => usuario.PasswordHash).HasMaxLength(255).IsRequired();
            builder.Property(usuario => usuario.NombreCompleto).HasMaxLength(150);
            builder.Property(usuario => usuario.Activo).HasDefaultValue(true);
            builder.Property(usuario => usuario.AccessFailedCount).HasDefaultValue(0);
            builder.Property(usuario => usuario.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(usuario => usuario.Email).IsUnique().HasDatabaseName("UQ_Usuarios_Email");
            builder.HasOne(usuario => usuario.Persona)
                .WithMany(persona => persona.Usuarios)
                .HasForeignKey(usuario => usuario.IdPersona)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UsuarioRol>(builder =>
        {
            builder.ToTable("UsuarioRol");
            builder.HasKey(usuarioRol => usuarioRol.IdUsuarioRol);
            builder.Property(usuarioRol => usuarioRol.FechaAsignacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(usuarioRol => new { usuarioRol.IdUsuario, usuarioRol.IdRol })
                .IsUnique()
                .HasDatabaseName("UQ_UsuarioRol_Usuario_Rol");
            builder.HasOne(usuarioRol => usuarioRol.Usuario)
                .WithMany(usuario => usuario.UsuarioRoles)
                .HasForeignKey(usuarioRol => usuarioRol.IdUsuario)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(usuarioRol => usuarioRol.Rol)
                .WithMany(rol => rol.UsuarioRoles)
                .HasForeignKey(usuarioRol => usuarioRol.IdRol)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(refreshToken => refreshToken.IdRefreshToken);
            builder.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(128).IsRequired();
            builder.Property(refreshToken => refreshToken.ReemplazadoPorTokenHash).HasMaxLength(128);
            builder.Property(refreshToken => refreshToken.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.Ignore(refreshToken => refreshToken.EstaActivo);
            builder.HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique()
                .HasDatabaseName("UQ_RefreshTokens_TokenHash");
            builder.HasIndex(refreshToken => new { refreshToken.IdUsuario, refreshToken.FechaExpiracion })
                .HasDatabaseName("IX_RefreshTokens_Usuario_Expiracion");
            builder.HasOne(refreshToken => refreshToken.Usuario)
                .WithMany(usuario => usuario.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(builder =>
        {
            builder.ToTable("Roles");
            builder.HasKey(rol => rol.IdRol);
            builder.Property(rol => rol.Nombre).HasMaxLength(50).IsRequired();
            builder.Property(rol => rol.Descripcion).HasMaxLength(200);
            builder.Property(rol => rol.Activo).HasDefaultValue(true);
            builder.HasIndex(rol => rol.Nombre).IsUnique().HasDatabaseName("UQ_Roles_Nombre");
            builder.HasData(
                new Rol { IdRol = 1, Nombre = "ORGANIZADOR", Descripcion = "Usuario encargado de administrar usuarios, roles y credenciales.", Activo = true },
                new Rol { IdRol = 2, Nombre = "JUGADOR", Descripcion = "Usuario autenticado para consultar informacion deportiva permitida.", Activo = true });
        });
    }
}
