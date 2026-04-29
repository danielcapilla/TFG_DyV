using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization;

public class CountdownIntro : NetworkBehaviour
{
    [Header("Duracion")]
    [SerializeField] private float countdownSeconds = 3f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Localizacion")]
    [SerializeField] private LocalizedStringTable localizedStringTable;

    public event EventHandler OnCountdownFinished;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;
    private Vector3 originalScale;
    private Vector3 scaleTo;

    private void Awake()
    {
        if (countdownText != null)
        {
            originalScale = countdownText.transform.localScale;
            scaleTo = originalScale * 1.5f;
        }
        if (!IsOffline) return;
        ChooseGroup.OnGameStartEvent -= OnGameStart;
        ChooseGroup.OnGameStartEvent += OnGameStart;
    }

    private void OnDestroy()
    {
        if (IsOffline)
            ChooseGroup.OnGameStartEvent -= OnGameStart;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            ChooseGroup.OnGameStartEvent -= OnGameStart;
            ChooseGroup.OnGameStartEvent += OnGameStart;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
            ChooseGroup.OnGameStartEvent -= OnGameStart;
    }

    private void OnGameStart() => StartCountdown();

    private void StartCountdown()
    {
        StartCoroutine(RunCountdown());
        if (!IsOffline && IsServer)
            StartCountdownClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    private void StartCountdownClientRpc() => StartCoroutine(RunCountdown());

    private IEnumerator RunCountdown()
    {
        float remaining = countdownSeconds;
        while (remaining > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = remaining.ToString("0");
                OnScale(countdownText.transform);
            }
            yield return new WaitForSeconds(1.3f);
            remaining -= 1f;
        }

        if (countdownText != null && localizedStringTable != null)
        {
            countdownText.text = localizedStringTable.GetTable().GetEntry("PreCountdown").GetLocalizedString();
            OnScale(countdownText.transform);
            yield return new WaitForSeconds(1.3f);
        }

        if (countdownText != null)
            countdownText.text = "";

        OnCountdownFinished?.Invoke(this, EventArgs.Empty);
    }

    private void OnScale(Transform gui)
    {
        gui.DOScale(scaleTo, 0.5f).SetEase(Ease.InOutSine)
            .OnComplete(() => gui.DOScale(originalScale, 0.5f).SetEase(Ease.InOutSine));
    }
}
