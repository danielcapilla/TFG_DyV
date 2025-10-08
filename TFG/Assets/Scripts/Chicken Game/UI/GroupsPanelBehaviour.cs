using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupsPanelBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button[] groupButtons;

    [Header("Aparición Ready")]
    [SerializeField] private float readyAppearDuration = 0.45f;
    [SerializeField] private float readyOvershootScale = 1.15f;

    [Header("Pulso Ready")]
    [SerializeField] private float pulseScale = 1.08f;
    [SerializeField] private float pulseDuration = 0.8f;
    [SerializeField] private Ease pulseEase = Ease.InOutSine;

    [Header("Movimiento Idle Botones Grupos")]
    [SerializeField] private float groupMoveRadius = 8f;
    [SerializeField] private Vector2 groupMoveDurationRange = new Vector2(2.5f, 4.2f);
    [SerializeField] private float groupRandomStartDelayMax = 0.6f;
    [SerializeField] private Ease groupMoveEase = Ease.InOutSine;

    [Header("Idle Grupos")]
    [SerializeField] private float floatAmplitude = 6f;
    [SerializeField] private float floatDuration = 2.4f;
    [SerializeField] private float rotationAngle = 6f;
    [SerializeField] private float rotationDuration = 3.2f;
    [SerializeField] private float scalePulse = 1.05f;
    [SerializeField] private float scaleDuration = 1.6f;
    [SerializeField] private float desyncMaxDelay = 0.6f;

    private readonly List<Vector2> _originalPositions = new List<Vector2>();
    private Sequence _readySequence;
    private Tweener _readyPulseTween;
    private readonly List<Tweener> _groupTweens = new List<Tweener>();
    private bool _readyShownOnce = false;

    void Awake()
    {
        CacheOriginalPositions();
    }

    void OnEnable()
    {
        StartGroupIdleMotion();
        if (readyButton != null && readyButton.gameObject.activeSelf && !_readyShownOnce)
            PlayReadyAppear();
    }

    void OnDisable()
    {
        KillAllTweens();
    }

    void OnDestroy()
    {
        KillAllTweens();
    }

    private void CacheOriginalPositions()
    {
        _originalPositions.Clear();
        if (groupButtons == null) return;
        foreach (var b in groupButtons)
        {
            if (b == null) { _originalPositions.Add(Vector2.zero); continue; }
            RectTransform rt = b.transform as RectTransform;
            _originalPositions.Add(rt != null ? rt.anchoredPosition : Vector2.zero);
        }
    }

    public void PlayReadyAppear()
    {
        if (readyButton == null) return;

        _readyShownOnce = true;

        // Limpia tweens previos
        if (_readySequence != null && _readySequence.IsActive()) _readySequence.Kill();
        if (_readyPulseTween != null && _readyPulseTween.IsActive()) _readyPulseTween.Kill();

        RectTransform rt = readyButton.transform as RectTransform;
        rt.localScale = Vector3.zero;

        _readySequence = DOTween.Sequence()
            .Append(rt.DOScale(readyOvershootScale, readyAppearDuration * 0.65f)
                .SetEase(Ease.OutBack))
            .Append(rt.DOScale(1f, readyAppearDuration * 0.35f)
                .SetEase(Ease.InOutSine))
            .OnComplete(StartReadyPulse);
    }

    private void StartReadyPulse()
    {
        if (readyButton == null) return;
        RectTransform rt = readyButton.transform as RectTransform;
        _readyPulseTween = rt.DOScale(pulseScale, pulseDuration / 2f)
            .SetEase(pulseEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartGroupIdleMotion()
    {
        KillGroupTweens();
        if (groupButtons == null) return;

        for (int i = 0; i < groupButtons.Length; i++)
        {
            var btn = groupButtons[i];
            if (btn == null) continue;
            RectTransform rt = btn.transform as RectTransform;
            if (rt == null) continue;

            Vector2 origin = (i < _originalPositions.Count) ? _originalPositions[i] : rt.anchoredPosition;
            rt.anchoredPosition = origin;

            float delay = Random.Range(0f, desyncMaxDelay);
            float dir = Random.value > 0.5f ? 1f : -1f;

            // Flotacion vertical suave
            var floatTween = rt.DOAnchorPosY(origin.y + floatAmplitude, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay);
            _groupTweens.Add(floatTween);

            // Rotacion balanceo
            var rotTween = rt.DOLocalRotate(new Vector3(0f, 0f, rotationAngle * dir), rotationDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay * 0.5f);
            _groupTweens.Add(rotTween);

            // Pulso suave de escala
            var scaleTween = rt.DOScale(scalePulse, scaleDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay * 0.3f);
            _groupTweens.Add(scaleTween);
        }
    }

    private void KillGroupTweens()
    {
        foreach (var t in _groupTweens)
        {
            if (t != null && t.IsActive())
                t.Kill();
        }
        _groupTweens.Clear();
    }

    private void KillAllTweens()
    {
        if (_readySequence != null && _readySequence.IsActive()) _readySequence.Kill();
        if (_readyPulseTween != null && _readyPulseTween.IsActive()) _readyPulseTween.Kill();
        KillGroupTweens();
    }

}
