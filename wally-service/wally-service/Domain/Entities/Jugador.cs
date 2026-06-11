using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class Jugador : Entity
{
    public int IdJugador { get; set; }

    public int? IdPersona { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<InscripcionEquipoJugador> Inscripciones { get; set; } = [];
}
