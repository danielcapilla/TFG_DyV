using Unity.Netcode;
using UnityEngine;
using TMPro;
using System;

public class TurnTimer : NetworkBehaviour
{
    [SerializeField] private float timerDuration = 10f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TeamMenager teamMenager;
    [SerializeField] private GameManagerChicken gameManagerChicken;

    private float currentTime;
    private bool isTimerRunning = false;

    public override void OnNetworkSpawn()
    {
        
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned += StartTimers;
        }
        
    }

    private void StartTimers(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        // Obtener todos los clientes del grupo especifico
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        //playerNOR.TryGet(out NetworkObject playerNetworkObject);
        //PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();
        teamInfo.time = 10f;
        if (targetClients.Length == 0) return; // Si no hay clientes en el grupo, salir
        ObtainTimeForGroupClientRPC(timerDuration,new ClientRpcParams
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
        currentTime = time;
        isTimerRunning = true;
        UpdateTimerDisplay(currentTime);
    }

    private void StartTimers()
    {
        Debug.Log("Iniciando timers para todos los jugadores");
        isTimerRunning = true;
        int groupId = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamInfoChicken teamInfoChicken = (TeamInfoChicken)teamMenager.teams[groupId];
        teamInfoChicken.time = timerDuration;
        currentTime = timerDuration;
        UpdateTimerDisplay(currentTime);
        
    }

    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerDisplay(currentTime);

        //if (currentTime <= 0f)
        //{
        //    currentTime = 0f;
        //    isTimerRunning = false;
        //    TimerCompleted();
        //}
    }

    public void StartTimer()
    {
        //currentTime = timerDuration;
        isTimerRunning = true;
        int groupId = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        Debug.Log($"Grupo actual: {groupId}");
        TeamInfoChicken teamInfoChicken = (TeamInfoChicken)teamMenager.teams[groupId];
        teamInfoChicken.time = timerDuration;
    }

    private void UpdateTimerDisplay(float time)
    {
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{seconds:00}";
    }

    private void TimerCompleted()
    {
        timerText.text = "00";
        Debug.Log("Timer terminado!");
    }
}