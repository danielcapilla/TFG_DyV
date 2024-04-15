using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnOffInstance : NetworkBehaviour
{
    private MeshRenderer visual;

    private void Start()
    {
        if(IsHost || IsServer) 
        {
            visual = GetComponent<MeshRenderer>();
            if (!HostManager.activarEscena)
            {
                visual.enabled = false;
            }
        }
    }
}
