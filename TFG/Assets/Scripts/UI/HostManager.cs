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
        //teamManager.teams[0].onPuntuacionChanged += ActualizarScore;
        //TurnOffVisuals();
    }
    public override void OnNetworkDespawn()
    {
        cameraSelector.OnCameraChange -= ChangeScore;
    }

    private int prevCamera = 0;
    private void ChangeScore(object sender, CameraSelector.OnCameraChangeEventArgs e)
    {
        teamManager.teams[prevCamera].onPuntuacionChanged -= ActualizarScore;
        TeamInfo team = teamManager.teams[e.cameraID];
        scoreText.text = team.Puntuacion.ToString();
        team.onPuntuacionChanged += ActualizarScore;
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

    public void ActualizarScore(int valor)
    {
        scoreText.text = valor.ToString();
    }
}
