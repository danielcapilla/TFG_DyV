using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Countdown : NetworkBehaviour
{
    [SerializeField]
    private float tiempo;
    public NetworkVariable<bool> timeStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private TextMeshProUGUI GUI;
    [SerializeField]
    private float max;
    [SerializeField]
    private Image fill;

    //private LateJoinsBehaviour lateJoinsBehaviour;
    private void Start()
    {
        //tiempo = 3.0f;
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
        fill.fillAmount = tiempo / max;

        if (tiempo <= 0)
        {
            tiempo = 0;
            if (IsServer )
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
