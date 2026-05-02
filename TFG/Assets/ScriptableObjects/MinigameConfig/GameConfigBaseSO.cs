using UnityEngine;

/// <summary>
/// Configuracion base compartida por todos los minijuegos.
/// Crear subclases para añadir configuracion especifica de cada juego.
/// </summary>
public abstract class GameConfigBaseSO : ScriptableObject
{
    [Header("General")]
    public float matchDuration = 180f;
    public int   rounds        = 1;
    public int   maxPlayers    = 4;
}
