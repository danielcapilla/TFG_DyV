using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Base reutilizable para gestores de minijuego.
/// Activa el PlayerController cuando CountdownIntro termina (no antes).
/// </summary>
public abstract class GameManagerBase : NetworkBehaviour
{
    [Header("Base - Referencias")]
    [SerializeField] protected CameraSelector cameraSelector;
    [SerializeField] protected MatchTimer matchTimer;
    [SerializeField] protected CountdownIntro countdownIntro;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    // ── Ciclo de vida online ──────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            if (countdownIntro != null)
                countdownIntro.OnCountdownFinished += OnCountdownFinishedHandler;
            if (matchTimer != null)
                matchTimer.OnTimerFinished += OnTimerFinished;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            if (countdownIntro != null)
                countdownIntro.OnCountdownFinished -= OnCountdownFinishedHandler;
            if (matchTimer != null)
                matchTimer.OnTimerFinished -= OnTimerFinished;
        }
    }

    // ── Ciclo de vida offline ─────────────────────────────────────────────────

    private void Awake()
    {
        if (!IsOffline) return;
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished += OnCountdownFinishedHandler;
        if (matchTimer != null)
            matchTimer.OnTimerFinished += OnTimerFinished;
    }

    private void OnDestroy()
    {
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished -= OnCountdownFinishedHandler;
        if (matchTimer != null)
            matchTimer.OnTimerFinished -= OnTimerFinished;
    }

    // ── Arranque de partida (cuando la cuenta atras termina) ──────────────────

    private void OnCountdownFinishedHandler(object sender, EventArgs e)
    {
        List<ulong> players = new List<ulong>(ChooseGroup.connectedPlayers ?? new List<ulong>());
        AllowGameStart(players);
    }

    private void AllowGameStart(List<ulong> players)
    {
        if (IsOffline)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (pc != null) pc.enabled = true;
        }
        else
        {
            foreach (ulong playerId in players)
            {
                if (!NetworkManager.ConnectedClients.ContainsKey(playerId)) continue;
                NetworkObject playerStats = NetworkManager.ConnectedClients[playerId].PlayerObject;
                if (playerStats == null) continue;

                SetCameraRpc(playerStats.GetComponent<PlayerStats>().idGrupo.Value, playerId);

                if (playerStats.transform.childCount > 0)
                {
                    NetworkObject playerNetObj = playerStats.transform.GetChild(0)
                        .GetComponent<NetworkObject>();
                    if (playerNetObj != null)
                        ActivatePlayerControllerRpc(playerNetObj);
                }
            }
        }

        OnGameStarted();
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerControllerRpc(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNetObj)) return;
        PlayerController controller = playerNetObj.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;
    }

    [Rpc(SendTo.Everyone)]
    private void SetCameraRpc(int groupID, ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId) return;
        cameraSelector.ActivateCamera(groupID);
    }

    // ── Metodos virtuales para subclases ──────────────────────────────────────

    /// <summary>Se llama cuando la cuenta atras termina y el juego arranca.</summary>
    protected virtual void OnGameStarted() { }

    /// <summary>Se llama cuando el timer llega a 0. Sobrescribir para guardar BD, cambiar escena, etc.</summary>
    protected virtual void OnTimerFinished() { }
}
