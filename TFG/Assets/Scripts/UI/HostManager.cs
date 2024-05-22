using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField]
    private CameraSelector cameraSelector;
    [SerializeField]
    private TeamMenager teamManager;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    public override void OnNetworkSpawn()
    {

        if (!IsServer)
        {
            hostCanvas.enabled = false;
            return;
        }
        cameraSelector.OnCameraChange += ChangeScore;
        //TurnOffVisuals();
    }
    public override void OnNetworkDespawn()
    {
        cameraSelector.OnCameraChange -= ChangeScore;
    }
    private void ChangeScore(object sender, CameraSelector.OnCameraChangeEventArgs e)
    {
        scoreText.text = teamManager.teams[e.cameraID].Puntuacion.ToString();
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
