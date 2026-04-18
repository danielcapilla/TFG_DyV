using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Implementa esta interfaz en cualquier objeto que el jugador pueda llevar consigo.
/// Solo describe el comportamiento DEL OBJETO mientras es transportado.
/// El entorno (ICarryReceiver) decide que hacer cuando lo recibe.
/// </summary>
public interface ICarryObject
{
    NetworkObject GetNetworkObject();
    GameObject GetGameObject();

    /// <summary>Identificador de tipo de objeto (ej: 'baldosa_roja', 'plato').
    /// El receptor lo usa para validar si acepta este tipo.</summary>
    string CarryType { get; }

    /// <summary>Si false, nadie puede coger este objeto.</summary>
    bool CanBePickedUp { get; }

    /// <summary>Llamado cuando el jugador coge el objeto.</summary>
    void OnPickedUp(PlayerCarry carrier);

    /// <summary>Llamado cuando un receptor acepta y recibe el objeto.</summary>
    void OnDelivered(ICarryReceiver receiver);

    /// <summary>Llamado si el receptor rechaza el objeto (el jugador lo sigue llevando).</summary>
    void OnRejected(ICarryReceiver receiver);
}
