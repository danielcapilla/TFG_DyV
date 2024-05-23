using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HostManager : NetworkBehaviour
{
    private MeshRenderer[] objetosActivos;
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
    [SerializeField]
    private RecipeRandomizer recipeRandomizer;
    public override void OnNetworkSpawn()
    {

        if (!IsServer)
        {
            hostCanvas.enabled = false;
            return;
        }
        cameraSelector.OnCameraChange += ChangeScore;
        //teamManager.teams[0].onPuntuacionChanged += ActualizarScore;
        //Si se queda focus, no se actualiza la puntuacion ni las comandas
        //TurnOffVisuals();
    }
    public override void OnNetworkDespawn()
    {
        cameraSelector.OnCameraChange -= ChangeScore;
    }

    private int prevCamera = 0;
    private void ChangeScore(object sender, CameraSelector.OnCameraChangeEventArgs e)
    {
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamManager.teams[prevCamera];
        teamInfo.onPuntuacionChanged -= ActualizarScore;
        teamInfo.OnIdOrderChange -= ActualizarComanda;
        TeamInfoRestaurante team = (TeamInfoRestaurante)teamManager.teams[e.cameraID];
        scoreText.text = team.Puntuacion.ToString();
        team.onPuntuacionChanged += ActualizarScore;
        recipeRandomizer.NextOrder(team.idOrder);
        team.OnIdOrderChange += ActualizarComanda;
        prevCamera = e.cameraID;
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
            //Controles de cámara
            hostCanvas.transform.GetChild(1).gameObject.SetActive(false);
            hostCanvas.transform.GetChild(2).gameObject.SetActive(false);
            //Activar estadísticas
            hostCanvas.transform.GetChild(3).gameObject.SetActive(true);
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
            //Controles de cámara
            hostCanvas.transform.GetChild(1).gameObject.SetActive(true);
            hostCanvas.transform.GetChild(2).gameObject.SetActive(true);
            //Desactivar estadísticas
            hostCanvas.transform.GetChild(3).gameObject.SetActive(false);

        }

    }

    private void ActualizarScore(int valor)
    {
        scoreText.text = valor.ToString();
    }
    private void ActualizarComanda(int valor)
    {
        recipeRandomizer.NextOrder(valor);
    }
}
