using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] Transform CarryPosition;
    public bool isCarrying = false;
    public CarryObject carryingObject;

    [ServerRpc(RequireOwnership = false)]
    public void CarryObjectServerRPC(NetworkObjectReference carryObjectNetworkObjectReference) 
    {
        CarryObjectClientRPC(carryObjectNetworkObjectReference);
        isCarrying = true;
    }
    [ClientRpc]
    private void CarryObjectClientRPC(NetworkObjectReference carryObjectNetworkObjectReference)
    {
        carryObjectNetworkObjectReference.TryGet(out NetworkObject carryObjectNetworkObject);
        CarryObject carryObject = carryObjectNetworkObject.GetComponent<CarryObject>();
        carryObject.transform.parent = CarryPosition;
        carryObject.transform.localPosition = Vector3.zero;
        carryingObject = carryObject;
    }
    public CarryObject dropObject()
    {
        CarryObject temp = carryingObject;
        if (carryingObject)
        {
            carryingObject = null;
            temp.transform.parent = null;
        }
        //Give object to other script
        isCarrying=false;
        return temp;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
    //// INetworkSerializable
    //public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //{
    //    serializer.SerializeValue(ref isCarrying);
    //    //serializer.SerializeValue(ref carryingObject);
    //}
    //// ~INetworkSerializable
}
