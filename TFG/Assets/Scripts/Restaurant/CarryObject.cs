using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class CarryObject : NetworkBehaviour
{
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
    //private Transform targetTransform;

    //public void SetTargetTransform(Transform targetTransform)
    //{
    //    this.targetTransform = targetTransform;
    //}
    //private void LateUpdate()
    //{
    //    if (targetTransform == null) return;

    //    transform.position = targetTransform.position;

    //}
}
