using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserNetworkConfig : NetworkBehaviour
{


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
    }
    

}
