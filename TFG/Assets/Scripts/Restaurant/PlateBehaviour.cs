using DG.Tweening;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlateBehaviour : NetworkBehaviour, ICarryObject
{
    public List<IngredientBehaviour> Ingredients = new List<IngredientBehaviour>();

    // ── ICarryObject ──────────────────────────────────────────────────────────
    public string CarryType => "plate";
    public bool CanBePickedUp => true;
    public NetworkObject GetNetworkObject() => NetworkObject;
    public GameObject GetGameObject() => gameObject;
    public void OnPickedUp(PlayerCarry carrier) { }
    public void OnDelivered(ICarryReceiver receiver) { }
    public void OnRejected(ICarryReceiver receiver) { }

    // ── Añadir ingrediente ────────────────────────────────────────────────────

    public void AddIngredient(IngredientBehaviour ingredient, FixedString64Bytes playerName)
    {
        bool isOffline = !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

        if (isOffline)
        {
            AddIngredientLocally(ingredient, playerName);
        }
        else
        {
            NetworkObject no = ingredient.GetNetworkObject();
            if (no != null)
                AddIngredientServerRPC(no, playerName);
            else
                AddIngredientLocally(ingredient, playerName);
        }
    }

    private void AddIngredientLocally(IngredientBehaviour ingredient, FixedString64Bytes playerName)
    {
        ingredient.playerName = playerName;
        PlayerCarry.SetParentSafe(ingredient.gameObject, transform);
        PlaceIngredient(ingredient);
        Ingredients.Add(ingredient);
    }

    private void PlaceIngredient(IngredientBehaviour ingredient)
    {
        if (Ingredients.Count > 0)
        {
            IngredientBehaviour last = Ingredients[Ingredients.Count - 1];
            ingredient.transform.localPosition = new Vector3(
                0,
                last.transform.localPosition.y + last.transform.localScale.y * 2,
                0);
        }
        else
        {
            ingredient.transform.localPosition = new Vector3(0, ingredient.transform.localScale.y, 0);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddIngredientServerRPC(NetworkObjectReference ingredientRef, FixedString64Bytes playerName)
    {
        if (!ingredientRef.TryGet(out NetworkObject ingredientNetObj)) return;
        IngredientBehaviour ingredient = ingredientNetObj.GetComponent<IngredientBehaviour>();
        if (ingredient == null) return;

        PlayerCarry.SetParentSafe(ingredient.gameObject, transform);
        ingredient.playerName = playerName;
        RelocateClientRPC(ingredientRef);
    }

    [ClientRpc]
    private void RelocateClientRPC(NetworkObjectReference ingredientRef)
    {
        if (!ingredientRef.TryGet(out NetworkObject ingredientNetObj)) return;
        IngredientBehaviour ingredient = ingredientNetObj.GetComponent<IngredientBehaviour>();
        if (ingredient == null) return;

        // Reparentar localmente al plato en cada cliente (igual que SetParentSafe)
        PlayerCarry.SetParentSafe(ingredient.gameObject, transform);
        PlaceIngredient(ingredient);
        Ingredients.Add(ingredient);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        DOTween.Kill(transform);
    }
}
