using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostManager : NetworkBehaviour
{
    public MeshRenderer[] objetosActivos;
    public static bool activarEscena = true;
    [SerializeField]
    private GameObject orderCanvas;
    [SerializeField]
    private Canvas hostCanvas;

    public override void OnNetworkSpawn()
    {

        if (!IsServer)
        {
            hostCanvas.enabled = false;
            return;
        }
        //TurnOffVisuals();
    }


    public void TurnOffVisuals()
    {
        if (!IsServer) return;
        if (activarEscena)
        {
            activarEscena = false;
            objetosActivos = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer objeto in objetosActivos)
            {
                objeto.enabled = false;
            }
            orderCanvas.SetActive(false);   
        }
        else
        {
            activarEscena = true;
            objetosActivos = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer objeto in objetosActivos)
            {
                objeto.enabled = true;
            }
            orderCanvas.SetActive(true);
        }
        
    }
}
