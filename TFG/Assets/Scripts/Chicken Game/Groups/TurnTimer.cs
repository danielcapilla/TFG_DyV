using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;

public class TurnTimer : NetworkBehaviour
{
    [Header("Variables")]
    [SerializeField] private float timerDuration = 10f;
    [SerializeField] private float delayAfterZero = 1.5f;

    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TeamMenager teamMenager;
    [SerializeField] private GameManagerChicken gameManagerChicken;
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private TurnBehaviour turnBehaviour;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isDelayRunning = false;
    public Action<ulong> OnTimerEnd;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned += StartTimers;
            groupBehaviour.OnExecutedTurn += (playerInput, groupId) => StartTimers(playerInput.NetworkObject, groupId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned -= StartTimers;
            groupBehaviour.OnExecutedTurn -= (playerInput, groupId) => StartTimers(playerInput.NetworkObject, groupId);
        }
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
        StartCoroutine(DelayAndStartTimer(time));
    }

    void Update()
    {
        if (!isTimerRunning || isDelayRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerDisplay(currentTime);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            CheckTurnRPC(NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value);
            //StartCoroutine(DelayAndNextTurn());
        }
    }
    [Rpc(SendTo.Server)]
    private void CheckTurnRPC(int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];

        int turnIndex = teamInfo.turn;  
        ulong turnClientId = teamInfo.integrantes[turnIndex];

        List<ulong> otherClients = new List<ulong>(teamInfo.integrantes);
        otherClients.Remove(turnClientId);
        // El cliente que tiene el turno, reinicia el timer y ejecuta el siguiente turno
        ResetAndNextTurnClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { turnClientId } }
        });

        // Los demas solo reinician el timer
        if (otherClients.Count > 0)
        {
            ResetTimerClientRPC(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = otherClients.ToArray() }
            });
        }
    }
    [ClientRpc]
    private void ResetTimerClientRPC(ClientRpcParams clientRpcParams)
    {
        StartCoroutine(DelayAndStartTimer(timerDuration));
    }

    [ClientRpc]
    private void ResetAndNextTurnClientRPC(ClientRpcParams clientRpcParams)
    {      
        StartCoroutine(DelayAndNextTurn());
    }

    private IEnumerator DelayAndStartTimer(float time)
    {
        isDelayRunning = true;
        UpdateTimerDisplay(0f);
        yield return new WaitForSeconds(delayAfterZero);

        currentTime = time;
        isTimerRunning = true;
        isDelayRunning = false;
        UpdateTimerDisplay(currentTime);
    }

    private IEnumerator DelayAndNextTurn()
    {
        isDelayRunning = true;
        UpdateTimerDisplay(0f);
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
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{seconds:00}";
    }
}