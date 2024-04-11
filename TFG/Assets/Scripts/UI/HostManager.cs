using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostManager : NetworkBehaviour
{
    public MeshRenderer[] objetosActivos;
    public static bool activarEscena;
    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;
        TurnOffVisuals();
    }

    public void TurnOffVisuals()
    {
        if(activarEscena)
        {
            activarEscena = false;
            objetosActivos = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer objeto in objetosActivos)
            {
                objeto.enabled = false;
            }
        }
        else
        {
            activarEscena = true;
            objetosActivos = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer objeto in objetosActivos)
            {
                objeto.enabled = true;
            }
        }
        
    }
}
