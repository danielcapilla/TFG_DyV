using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IngredientBehaviour : NetworkBehaviour, ICarryObject
{
    public IngredientsScriptableObject ingredient;
    public FixedString64Bytes playerName;

    public string CarryType => ingredient != null ? ingredient.name : "ingredient";
    public bool CanBePickedUp => true;

    public NetworkObject GetNetworkObject() => NetworkObject;
    public GameObject GetGameObject() => gameObject;

    public void OnPickedUp(PlayerCarry carrier) { }
    public void OnDelivered(ICarryReceiver receiver) { }
    public void OnRejected(ICarryReceiver receiver) { }
}
