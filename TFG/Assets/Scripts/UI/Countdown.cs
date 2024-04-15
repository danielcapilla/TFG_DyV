using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;

public class Countdown : NetworkBehaviour
{
    //Variable compartida tiempo
    //public NetworkVariable<float> timeNet = new();
    float tiempo;
    public NetworkVariable<bool> timeStarted = new();
    [SerializeField] TextMeshProUGUI GUI;
    //public GameObject contador;
    
    private void Start()
    {
        //timeNet.Value = 30.0f;
        tiempo = 120.0f;
        timeStarted.Value = false;

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
            Debug.Log("Se acabó el tiempo!!!");
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
