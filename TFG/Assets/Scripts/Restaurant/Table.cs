using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Table : InteractableObject
{
    bool isOccupied = false;
    [SerializeField] Transform placePosition;
    ICarryObject holdingObject;

    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null) return;
        if (!isOccupied && carry.isCarrying)      PlaceObjectOffline(carry);
        else if (isOccupied && !carry.isCarrying) PickUpFromTableOffline(carry);
        else if (isOccupied && carry.isCarrying)  TryAddIngredientToPlate(carry);
    }

    private void PlaceObjectOffline(PlayerCarry carry)
    {
        holdingObject = carry.DropObject();
        if (holdingObject == null) return;
        PlayerCarry.SetParentSafe(holdingObject.GetGameObject(), transform);
        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;
        isOccupied = true;
    }

    private void PickUpFromTableOffline(PlayerCarry carry)
    {
        if (holdingObject == null) return;
        carry.PickUpLocally(holdingObject);
        isOccupied = false;
        holdingObject = null;
    }

    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) ReplaceObjectsServerRPC(netObj);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReplaceObjectsServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null) return;

        if (!isOccupied && carry.isCarrying)
        {
            carry.carryingObject.GetNetworkObject().TrySetParent((NetworkObject)null);
            PlaceObjectClientRPC(playerRef, carry.carryingObject.GetNetworkObject());
        }
        else if (isOccupied && !carry.isCarrying)
        {
            holdingObject.GetNetworkObject().TrySetParent(playerNet);
            PickUpFromTableClientRPC(playerRef, holdingObject.GetNetworkObject());
        }
        else if (isOccupied && carry.isCarrying)
        {
            TryAddIngredientToPlate(carry);
        }
    }

    [ClientRpc]
    private void PlaceObjectClientRPC(NetworkObjectReference playerRef, NetworkObjectReference objRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!objRef.TryGet(out NetworkObject objNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        holdingObject = carry.DropObject();
        PlayerCarry.SetParentSafe(objNet.gameObject, transform);
        objNet.transform.localPosition = placePosition.localPosition;
        isOccupied = true;
    }

    [ClientRpc]
    private void PickUpFromTableClientRPC(NetworkObjectReference playerRef, NetworkObjectReference objRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!objRef.TryGet(out NetworkObject objNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        ICarryObject obj = objNet.GetComponent<ICarryObject>();
        if (obj == null) return;
        carry.SetCarryingObject(obj);
        PlayerCarry.SetParentSafe(objNet.gameObject, carry.GetCarryPosition());
        objNet.transform.localPosition = Vector3.zero;
        objNet.transform.localRotation = Quaternion.identity;
        obj.OnPickedUp(carry);
        isOccupied = false;
        holdingObject = null;
    }

    private void TryAddIngredientToPlate(PlayerCarry carry)
    {
        if (holdingObject == null || carry.carryingObject == null) return;
        if (!holdingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate)) return;
        if (!carry.carryingObject.GetGameObject().TryGetComponent<IngredientBehaviour>(out IngredientBehaviour _)) return;

        FixedString64Bytes username = IsOffline
            ? new FixedString64Bytes("offline")
            : (carry.GetComponentInParent<UserNetworkConfig>() != null
                ? carry.GetComponentInParent<UserNetworkConfig>().usernameNetworkVariable.Value
                : new FixedString64Bytes("unknown"));

        ICarryObject dropped = carry.DropObject();
        if (dropped is IngredientBehaviour ing)
            plate.AddIngredient(ing, username);
    }
}
