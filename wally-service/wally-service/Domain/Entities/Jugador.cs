using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class Jugador : Entity
{
    public int IdJugador { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<InscripcionEquipoJugador> Inscripciones { get; set; } = [];
}
