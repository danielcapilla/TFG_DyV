using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ICarryObject 
{
    public NetworkObject GetNetworkObject();
    
    public GameObject GetGameObject();
}
