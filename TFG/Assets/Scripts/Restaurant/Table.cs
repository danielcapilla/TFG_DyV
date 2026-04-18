using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Table : InteractableObject
{
    bool isOccupied = false;
    [SerializeField] Transform placePosition;
    ICarryObject holdingObject;

    protected override void InteractOffline(PlayerCarry player)
    {
        if (!isOccupied && player.isCarrying)       PlaceObjectOffline(player);
        else if (isOccupied && !player.isCarrying)  PickUpFromTableOffline(player);
        else if (isOccupied && player.isCarrying)   TryAddIngredientToPlate(player);
    }

    private void PlaceObjectOffline(PlayerCarry player)
    {
        holdingObject = player.DropObject();
        if (holdingObject == null) return;
        PlayerCarry.SetParentSafe(holdingObject.GetGameObject(), transform);
        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;
        isOccupied = true;
    }

    private void PickUpFromTableOffline(PlayerCarry player)
    {
        if (holdingObject == null) return;
        player.PickUpLocally(holdingObject);
        isOccupied = false;
        holdingObject = null;
    }

    protected override void InteractOnline(PlayerCarry player)
    {
        ReplaceObjectsServerRPC(player.GetNetworkObject());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReplaceObjectsServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();

        if (!isOccupied && playerCarry.isCarrying)
        {
            // Actualizar estado Netcode: null parent (root)
            playerCarry.carryingObject.GetNetworkObject().TrySetParent((NetworkObject)null);
            PlaceObjectClientRPC(playerRef, playerCarry.carryingObject.GetNetworkObject());
        }
        else if (isOccupied && !playerCarry.isCarrying)
        {
            // Actualizar estado Netcode: hijo del jugador
            holdingObject.GetNetworkObject().TrySetParent(playerNet);
            PickUpFromTableClientRPC(playerRef, holdingObject.GetNetworkObject());
        }
        else if (isOccupied && playerCarry.isCarrying)
        {
            TryAddIngredientToPlate(playerCarry);
        }
    }

    [ClientRpc]
    private void PlaceObjectClientRPC(NetworkObjectReference playerRef, NetworkObjectReference objRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!objRef.TryGet(out NetworkObject objNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        holdingObject = playerCarry.DropObject(); // DropObject ya hace SetParentSafe(null)
        // Ahora mover a la posicion de la mesa localmente
        PlayerCarry.SetParentSafe(objNet.gameObject, transform);
        objNet.transform.localPosition = placePosition.localPosition;
        isOccupied = true;
    }

    [ClientRpc]
    private void PickUpFromTableClientRPC(NetworkObjectReference playerRef, NetworkObjectReference objRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!objRef.TryGet(out NetworkObject objNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        ICarryObject obj = objNet.GetComponent<ICarryObject>();
        if (obj == null) return;
        playerCarry.SetCarryingObject(obj);
        PlayerCarry.SetParentSafe(objNet.gameObject, playerCarry.GetCarryPosition());
        objNet.transform.localPosition = Vector3.zero;
        objNet.transform.localRotation = Quaternion.identity;
        obj.OnPickedUp(playerCarry);
        isOccupied = false;
        holdingObject = null;
    }

    private void TryAddIngredientToPlate(PlayerCarry player)
    {
        if (holdingObject == null || player.carryingObject == null) return;
        if (!holdingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate)) return;
        if (!player.carryingObject.GetGameObject().TryGetComponent<IngredientBehaviour>(out IngredientBehaviour _)) return;

        FixedString64Bytes username = IsOffline
            ? new FixedString64Bytes("offline")
            : (player.GetComponentInParent<UserNetworkConfig>() != null
                ? player.GetComponentInParent<UserNetworkConfig>().usernameNetworkVariable.Value
                : new FixedString64Bytes("unknown"));

        ICarryObject dropped = player.DropObject();
        if (dropped is IngredientBehaviour ing)
            plate.AddIngredient(ing, username);
    }
}
