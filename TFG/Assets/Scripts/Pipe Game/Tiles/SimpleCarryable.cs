using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Implementacion minima de ICarryObject para poder coger la baldosa del suelo.
/// </summary>
public class SimpleCarryable : NetworkBehaviour, ICarryObject
{
    public string CarryType => "tile";
    public bool CanBePickedUp => true;

    public NetworkObject GetNetworkObject() => NetworkObject;
    public GameObject GetGameObject() => gameObject;

    public void OnPickedUp(PlayerCarry carrier) { }
    public void OnDelivered(ICarryReceiver receiver) { }
    public void OnRejected(ICarryReceiver receiver) { }
}
