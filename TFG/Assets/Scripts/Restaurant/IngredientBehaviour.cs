using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IngredientBehaviour : NetworkBehaviour, ICarryObject
{
    public IngredientsScriptableObject ingredient;
    public FixedString64Bytes playerName;
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
