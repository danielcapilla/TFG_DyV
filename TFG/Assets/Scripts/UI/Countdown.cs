using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class Countdown : NetworkBehaviour
{
    [SerializeField]
    private float tiempo;
    [SerializeField]
    private float regresiveTime;
    public NetworkVariable<bool> timeStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private TextMeshProUGUI GUI;
    [SerializeField]
    private float max;
    [SerializeField]
    private Image fill;
    private bool regresiveTimerFinished = false;
    private Vector3 originalScale;
    private Vector3 scaleTo;

    //private LateJoinsBehaviour lateJoinsBehaviour;
    private void Start()
    {
        //tiempo = 3.0f;
        timeStarted.Value = false;
        originalScale = GUI.transform.localScale;
        scaleTo = originalScale * 1.5f;
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
        if (!timeStarted.Value) { return; }
        if(!regresiveTimerFinished) { return; }
        tiempo -= Time.fixedDeltaTime;
        GUI.text = ((int)tiempo).ToString();
        fill.fillAmount = tiempo / max;

        if (tiempo <= 0)
        {
            tiempo = 0;
            if (IsServer)
            {
                //lateJoinsBehaviour.aprovedConection = true;
                NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
            }
        }
    }
    private IEnumerator RunTimer()
    {

        while (regresiveTime >= 0f)
        {
            GUI.text = (regresiveTime).ToString();
            OnScale();
            yield return new WaitForSeconds(1.3f); // Esperar un segundo antes de continuar
            regresiveTime -= 1f; // Reducir el tiempo restante
        }
        regresiveTimerFinished = true;
    }
    private void OnScale()
    {
        GUI.transform.DOScale(scaleTo, 0.5f).SetEase(Ease.InOutSine)
            .OnComplete(()=>
            {
                GUI.transform.DOScale(originalScale, 0.5f)
                .SetEase(Ease.InOutSine);
            });
    }
    public void CambiarVariable()
    {
        timeStarted.Value = true;
        StartRegresiveCountdownClientRPC();
    }
    [ClientRpc]
    private void StartRegresiveCountdownClientRPC()
    {
        StartCoroutine(RunTimer());
    }
}
