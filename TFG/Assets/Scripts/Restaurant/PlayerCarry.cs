using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCarry : NetworkBehaviour
{
    private Transform CarryPosition;
    public bool isCarrying = false;
    public CarryObject carryingObject;

    public override void OnNetworkSpawn()
    {
        CarryPosition = GetComponentInChildren<Transform>();    
        base.OnNetworkSpawn();
    }
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
        Debug.Log(carryObject.transform.parent);
        carryObject.transform.SetParent(transform);
        //NetworkObject.TrySetParent(carryObject.transform);
        //carryObjectNetworkObject.transform.parent = transform;
        //carryObjectNetworkObject.transform.localPosition = Vector3.zero;
        //carryObject.SetTargetTransform(CarryPosition);
        carryObject.transform.localPosition = CarryPosition.localPosition;
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
