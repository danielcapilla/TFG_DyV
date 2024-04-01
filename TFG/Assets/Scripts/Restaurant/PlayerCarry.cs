using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCarry : MonoBehaviour, INetworkSerializable
{
    [SerializeField] Transform CarryPosition;
    public bool isCarrying = false;
    public CarryObject carryingObject;

    public void carryObject(CarryObject carryObject) 
    {
        carryObject.transform.parent = CarryPosition;
        carryObject.transform.localPosition = Vector3.zero;
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

    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isCarrying);
        //serializer.SerializeValue(ref carryingObject);
    }
    // ~INetworkSerializable
}
