using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class TurnTimer : NetworkBehaviour
{
    [Header("Variables")]
    [SerializeField] private float timerDuration = 10f;
    [SerializeField] private float delayAfterZero = 1.5f;

    [Header("Referencias")]
    //[SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TeamMenager teamMenager;
    [SerializeField] private GameManagerChicken gameManagerChicken;
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private MovementPanelBehaviour panelBehaviour;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isDelayRunning = false;
    public Action<ulong> OnTimerEnd;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned += StartTimers;
            groupBehaviour.OnExecutedTurn += OnExecutedTurn;
            groupBehaviour.OnExecuteTurn += WaitForMovementCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned -= StartTimers;
            groupBehaviour.OnExecutedTurn -= OnExecutedTurn;
            groupBehaviour.OnExecuteTurn -= WaitForMovementCompleted;
        }
    }

    private void OnExecutedTurn(PlayerInputController playerInput, int groupId)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            return;

        var netObj = playerInput.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsSpawned)
            return;
        StartTimers(playerInput.NetworkObject, groupId);
    }
    private void WaitForMovementCompleted(int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        if (targetClients.Length == 0) return;
        SetTimerRunningClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        });
    }
    private void StartTimers(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        teamInfo.time = timerDuration;
        if (targetClients.Length == 0) return;
        ObtainTimeForGroupClientRPC(timerDuration, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        });
    }

    [ClientRpc]
    private void ObtainTimeForGroupClientRPC(float time, ClientRpcParams clientRpcParams)
    {
        // Cuando el servidor resetea el timer, espera el delay antes de arrancar el nuevo turno
        //StartCoroutine(DelayAndStartTimer(time));
        ResetTimer();
    }
    [ClientRpc]
    private void SetTimerRunningClientRPC(ClientRpcParams clientRpcParams)
    {
        isTimerRunning = false;
    }
    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerDisplay(currentTime);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            CheckTurnRPC(NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value, NetworkManager.LocalClientId);
            //StartCoroutine(DelayAndNextTurn());
        }
    }
    [Rpc(SendTo.Server)]
    private void CheckTurnRPC(int idGroup, ulong id)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];

        int turnIndex = teamInfo.turn;  
        ulong turnClientId = teamInfo.integrantes[turnIndex];
        if( turnClientId != id) return;
        //List<ulong> otherClients = new List<ulong>(teamInfo.integrantes);
        //otherClients.Remove(turnClientId);
        //// El cliente que tiene el turno, reinicia el timer y ejecuta el siguiente turno
        //ResetAndNextTurnClientRPC(new ClientRpcParams
        //{
        //    Send = new ClientRpcSendParams { TargetClientIds = new[] { turnClientId } }
        //});
        InvokeTimerEndClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { turnClientId } }
        });

        //// Los demas solo reinician el timer
        //if (otherClients.Count > 0)
        //{
        //    ResetTimerClientRPC(new ClientRpcParams
        //    {
        //        Send = new ClientRpcSendParams { TargetClientIds = otherClients.ToArray() }
        //    });
        //}
    }
    [ClientRpc]
    private void ResetTimerClientRPC(ClientRpcParams clientRpcParams)
    {
        //StartCoroutine(DelayAndStartTimer(timerDuration));
        ResetTimer();   
    }

    [ClientRpc]
    private void ResetAndNextTurnClientRPC(ClientRpcParams clientRpcParams)
    {      
        //StartCoroutine(DelayAndNextTurn());
        ResetTimerAndNextTurn();
    }

    private IEnumerator DelayAndStartTimer(float time)
    {
        isDelayRunning = true;
        UpdateTimerDisplay(timerDuration);
        yield return new WaitForSeconds(delayAfterZero);

        currentTime = time;
        isTimerRunning = true;
        isDelayRunning = false;
        UpdateTimerDisplay(currentTime);
    }
    private void ResetTimer()
    {
        currentTime = timerDuration;
        UpdateTimerDisplay(currentTime);
        isTimerRunning = true;

    }
    private void ResetTimerAndNextTurn()
    {
        currentTime = timerDuration;
        UpdateTimerDisplay(currentTime);
        isTimerRunning = true;
        OnTimerEnd?.Invoke(NetworkManager.LocalClientId);

    }
    [ClientRpc]
    private void InvokeTimerEndClientRPC(ClientRpcParams clientRpcParams)
    {
        OnTimerEnd?.Invoke(NetworkManager.LocalClientId);
    }
    private IEnumerator DelayAndNextTurn()
    {
        isDelayRunning = true;
        UpdateTimerDisplay(timerDuration);
        //Debug.Log($"Timer terminado, ejecutando siguiente turno. {NetworkManager.LocalClientId}");
        
        OnTimerEnd?.Invoke(NetworkManager.LocalClientId);
        yield return new WaitForSeconds(delayAfterZero);

        currentTime = timerDuration;
        isTimerRunning = true;
        isDelayRunning = false;
        UpdateTimerDisplay(currentTime);

    }
    private void UpdateTimerDisplay(float time)
    {
        //int seconds = Mathf.FloorToInt(time % 60f);
        //timerText.text = $"{seconds:00}";
        timerSlider.maxValue = timerDuration;
        timerSlider.value = Mathf.Clamp(time, 0f, timerDuration);

        panelBehaviour?.UpdateTimerUI(time, timerDuration);

        //OnTimerUpdated?.Invoke(time / timerDuration);
    }
}