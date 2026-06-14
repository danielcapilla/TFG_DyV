using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MatchTimer : NetworkBehaviour
{
    [System.Serializable]
    public class TimerDisplay
    {
        public TextMeshProUGUI timeText;
        public Image fillImage;
    }

    [Header("Duracion")]
    [SerializeField] private float duration = 180f;
    [Tooltip("Opcional: sobreescribe duration con config.matchDuration")]
    [SerializeField] private GameConfigBaseSO config;

    [Header("Displays (uno por cada panel de UI)")]
    [SerializeField] private List<TimerDisplay> displays = new();

    [Header("Referencias")]
    [SerializeField] private CountdownIntro countdownIntro;

    // Online: NetworkVariable sincroniza estado entre servidor y clientes
    public NetworkVariable<bool> IsRunning = new(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Offline: bool local
    private bool isRunningOffline = false;

    public event Action OnTimerFinished;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;
    private float remaining;
    private float max;

    private void Awake()
    {
        if (!IsOffline) return;
        if (config != null) duration = config.matchDuration;
        remaining = duration;
        max = duration;
        UpdateDisplays();
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished += OnCountdownFinished;
    }

    private void OnDestroy()
    {
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished -= OnCountdownFinished;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (config != null) duration = config.matchDuration;
        remaining = duration;
        max = duration;
        UpdateDisplays();
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished += OnCountdownFinished;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (countdownIntro != null)
            countdownIntro.OnCountdownFinished -= OnCountdownFinished;
    }

    private void OnCountdownFinished(object sender, EventArgs e)
    {
        // Si hay config de Pipe con tiempo infinito, no arrancar el timer
        var pipeConfig = config as PipeGameConfigSO;
        if (pipeConfig != null && pipeConfig.infiniteTime) return;
        StartTimer();
    }

    public void StartTimer()
    {
        if (IsOffline)
        {
            remaining = duration;
            max = duration;
            isRunningOffline = true;
            return;
        }
        if (!IsServer) return;
        if (config != null) duration = config.matchDuration;
        remaining = duration;
        max = duration;
        IsRunning.Value = true;
    }

    public void StopTimer()
    {
        if (IsOffline) { isRunningOffline = false; return; }
        if (!IsServer) return;
        IsRunning.Value = false;
    }

    private bool IsActive => IsOffline ? isRunningOffline : IsRunning.Value;

    private void FixedUpdate()
    {
        if (!IsActive) return;

        remaining -= Time.fixedDeltaTime;
        UpdateDisplays();

        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateDisplays();

            if (IsOffline)
            {
                isRunningOffline = false;
                OnTimerFinished?.Invoke();
            }
            else if (IsServer)
            {
                IsRunning.Value = false;
                OnTimerFinished?.Invoke();
            }
        }
    }

    private void UpdateDisplays()
    {
        var pipeConfig = config as PipeGameConfigSO;
        bool infinite  = pipeConfig != null && pipeConfig.infiniteTime;
        string text    = infinite ? "\u221e" : ((int)remaining).ToString();
        float fillAmount = infinite ? 1f : (max > 0f ? remaining / max : 0f);
        foreach (var d in displays)
        {
            if (d.timeText != null) d.timeText.text = text;
            if (d.fillImage != null) d.fillImage.fillAmount = fillAmount;
        }
    }
}
