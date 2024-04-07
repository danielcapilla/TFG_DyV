using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField]
    private Transform CarryPosition;
    public bool isCarrying = false;
    public ICarryObject carryingObject {  get;  private set; }

    public void CarryObject(ICarryObject carryObject)
    {
        CarryObjectServerRPC(carryObject.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    public void CarryObjectServerRPC(NetworkObjectReference carryObjectNetworkObjectReference) 
    {
        CarryObjectClientRPC(carryObjectNetworkObjectReference);       
    }
    [ClientRpc]
    public void CarryObjectClientRPC(NetworkObjectReference carryObjectNetworkObjectReference)
    {
        carryObjectNetworkObjectReference.TryGet(out NetworkObject carryObjectNetworkObject);
        ICarryObject carryObject = carryObjectNetworkObject.GetComponent<ICarryObject>();
        carryObjectNetworkObject.gameObject.transform.localPosition = CarryPosition.localPosition;
        carryingObject = carryObject;
        isCarrying = true;
    }
    public ICarryObject DropObject()
    {
        ICarryObject temp = carryingObject;
        if (carryingObject != null)
        {
            carryingObject = null;
            DropOnjectServerRPC(temp.GetNetworkObject());
            //temp.GetGameObject().transform.parent = null;
        }
        //give object to other script
        isCarrying = false;
        return temp;
    }
    [ServerRpc (RequireOwnership = false)]
    private void DropOnjectServerRPC(NetworkObjectReference tempNetworkObjectReference)
    {
        tempNetworkObjectReference.TryGet(out NetworkObject carryObjectNetworkObject);
        ICarryObject temp = carryObjectNetworkObject.GetComponent<ICarryObject>();

        temp.GetGameObject().transform.parent = null;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
