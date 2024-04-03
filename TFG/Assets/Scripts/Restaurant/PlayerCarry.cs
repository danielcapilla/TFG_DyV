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
    public CarryObject carryingObject;

    //public override void OnNetworkSpawn()
    //{
    //    CarryPosition = GetComponentInChildren<Transform>();    
    //    base.OnNetworkSpawn();
    //}
    public void CarryObject(CarryObject carryObject)
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
        CarryObject carryObject = carryObjectNetworkObject.GetComponent<CarryObject>();
        carryObjectNetworkObject.gameObject.transform.localPosition = CarryPosition.localPosition;
        carryingObject = carryObject;
        isCarrying = true;
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
