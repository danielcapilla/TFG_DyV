using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class Countdown : NetworkBehaviour
{
    float tiempo;
    public NetworkVariable<bool> timeStarted = new();
    [SerializeField] TextMeshProUGUI GUI;
    private LateJoinsBehaviour lateJoinsBehaviour;
    private void Start()
    {
        tiempo = 10.0f;
        timeStarted.Value = false;
        lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();
    }

    //NOS AHORRAMOS LA VARIABLE COMPARTIDA
    public override void OnNetworkSpawn()
    {
        //timeNet.OnValueChanged += comprobarTimer;
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

    //void comprobarTimer(float previous, float nuevo)
    //{
    //    timeNet.Value = nuevo;
    //    GUI.text = ((int)timeNet.Value).ToString();
    //    if (timeNet.Value <=0)
    //    {
    //        calcularWinnerServerRpc();
    //        cambiarEscenaClientRpc();
    //    }
    //}


    private void FixedUpdate()
    {
        //Usando solo una llamada Rpc, nos ahorramos una variable compartida
        if(!timeStarted.Value) { return; }
        tiempo -= Time.fixedDeltaTime;
        GUI.text = ((int)tiempo).ToString();
        if (tiempo <= 0)
        {
            lateJoinsBehaviour.aprovedConection = true;
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        //// como hago para que sea server? porq en weapon lo ahce
        //if (IsServer)
        //{
        //    //Debug.Log(timeNet + " Personaje " + gameObject.name);
        //    timeNet.Value -= Time.fixedDeltaTime;
        //    //Debug.Log(timeNet + " UI " + gameObject.name);
        //    GUI.text = ((int)timeNet.Value).ToString();
        //}
        //else
        //{
        //    //Debug.Log(timeNet + " UI " + gameObject.name);
        //    GUI.text = ((int)timeNet.Value).ToString();
        //}
    }
    //Informamos a los clientes del cambio de tiempo
    [ClientRpc]
    public void informarTiempoClientRpc()
    {
        tiempo -= Time.fixedDeltaTime;
        GUI.text = ((int)tiempo).ToString();
    } 

    public void CambiarVariable()
    {
        timeStarted.Value = true;
    }
}
