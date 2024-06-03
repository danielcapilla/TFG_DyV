using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlateBehaviour : NetworkBehaviour, ICarryObject
{
    public List<IngredientBehaviour> Ingredients = new List<IngredientBehaviour>();

    public void AddIngredient(IngredientBehaviour ingredient, FixedString64Bytes playerName) 
    {
        AddIngredientServerRPC(ingredient.GetNetworkObject(), playerName);
    }
    [ServerRpc (RequireOwnership = false)]
    private void AddIngredientServerRPC(NetworkObjectReference ingredientNetworkObjectReference, FixedString64Bytes playerName)
    {
        ingredientNetworkObjectReference.TryGet(out NetworkObject ingredientNetworkObject);
        IngredientBehaviour ingredient = ingredientNetworkObject.GetComponent<IngredientBehaviour>();

        ingredient.transform.parent = transform;
        RelocateClientRPC(ingredientNetworkObjectReference);
        ingredient.playerName = playerName;
        Ingredients.Add(ingredient);

    }
    [ClientRpc]
    private void RelocateClientRPC(NetworkObjectReference ingredientNetworkObjectReference)
    {
        ingredientNetworkObjectReference.TryGet(out NetworkObject ingredientNetworkObject);
        IngredientBehaviour ingredient = ingredientNetworkObject.GetComponent<IngredientBehaviour>();
        //ingredient.transform.localPosition = Vector3.zero;
        if (Ingredients.Count > 0)
        {
            ingredient.transform.localPosition = new Vector3(0, Ingredients[Ingredients.Count - 1].transform.localPosition.y + Ingredients[Ingredients.Count - 1].transform.localScale.y * 2, 0);
        }
        else
        {
            ingredient.transform.localPosition = new Vector3(0, this.transform.localPosition.y + ingredient.transform.localScale.y * 2, 0);
        }
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        DOTween.Kill(transform);
    }
}
