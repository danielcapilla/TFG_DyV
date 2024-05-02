using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class Countdown : NetworkBehaviour
{
    float tiempo;
    public NetworkVariable<bool> timeStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] TextMeshProUGUI GUI;

    //private LateJoinsBehaviour lateJoinsBehaviour;
    private void Start()
    {
        tiempo = 3.0f;
        timeStarted.Value = false;
        //lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();
    }

    public override void OnNetworkSpawn()
    {
        timeStarted.OnValueChanged += ComprobarTimeStarted;

    }
    public override void OnNetworkDespawn()
    {
        timeStarted.OnValueChanged -= ComprobarTimeStarted;
    }
    private void ComprobarTimeStarted(bool previousValue, bool newValue)
    {
        timeStarted.Value = newValue;
    }

    private void FixedUpdate()
    {       
                        
        //Usando solo una llamada Rpc, nos ahorramos una variable compartida
        if (!timeStarted.Value) { return; }
        tiempo -= Time.fixedDeltaTime;
        GUI.text = ((int)tiempo).ToString();
        if (tiempo <= 0)
        {
            if(IsServer )
            {
                //lateJoinsBehaviour.aprovedConection = true;
                NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            }
        }             

    }

    public void CambiarVariable()
    {
        timeStarted.Value = true;
    }
}
