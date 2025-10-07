using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovementPanelBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    public UIGradient gradient;

    public TextMeshProUGUI waitingText;
    public RectTransform panelTransform;

    public CanvasGroup canvasGroup; 
    public Image shadowBG;

    public CanvasGroup waitingPanel;
    public CanvasGroup movesPanel;

    private Graphic graphic;
    private bool isVisible = true;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    void Start()
    {
        // Panel gradiente
        DOTween.To(() => gradient.m_color1,
                   x => { gradient.m_color1 = x; if (graphic != null) graphic.SetAllDirty(); },
                   Color.cyan, 2f)
               .SetLoops(-1, LoopType.Yoyo);

        // Texto "Waiting..."
        waitingText.transform.localScale = Vector3.one;
        // Tween infinito con rebote suave
        waitingText.transform
            .DOScale(1.1f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        // Alternativa: Tween de fade
        //waitingText.DOFade(0.3f, 1.0f)
        //    .SetLoops(-1, LoopType.Yoyo)
        //    .SetEase(Ease.InOutSine);

        panelTransform.localScale = Vector3.zero;
        panelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }
    public void TogglePanel()
    {
        if (isVisible)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }

        isVisible = !isVisible;
    }

    private void ShowPanel()
    {
        gameObject.SetActive(true);

        // Escala y fade in
        panelTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.3f));
        seq.Join(panelTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        // Fondo oscuro (profundidad)
        if (shadowBG != null)
            shadowBG.DOColor(new Color(0, 0, 0, 0.5f), 0.3f);
    }

    private void HidePanel()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, 0.25f));
        seq.Join(panelTransform.DOScale(0.9f, 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => gameObject.SetActive(false));

        // Fondo oscuro
        if (shadowBG != null)
            shadowBG.DOColor(new Color(0, 0, 0, 0), 0.25f);
    }
    public void ShowMoves()
    {
        waitingPanel.DOFade(0, 0.3f);
        waitingPanel.transform.DOScale(0.95f, 0.3f).SetEase(Ease.InOutSine);
        waitingPanel.interactable = false;
        waitingPanel.blocksRaycasts = false;

        movesPanel.gameObject.SetActive(true);
        movesPanel.alpha = 0;
        movesPanel.transform.localScale = Vector3.one * 1.05f;

        DOVirtual.DelayedCall(0.3f, () =>
        {
            movesPanel.DOFade(1, 0.3f);
            movesPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            movesPanel.interactable = true;
            movesPanel.blocksRaycasts = true;
        });
    }

    public void ShowWaiting()
    {
        movesPanel.DOFade(0, 0.3f);
        movesPanel.transform.DOScale(0.95f, 0.3f).SetEase(Ease.InOutSine);
        movesPanel.interactable = false;
        movesPanel.blocksRaycasts = false;

        waitingPanel.gameObject.SetActive(true);
        waitingPanel.alpha = 0;
        waitingPanel.transform.localScale = Vector3.one * 1.05f;

        DOVirtual.DelayedCall(0.3f, () =>
        {
            waitingPanel.DOFade(1, 0.3f);
            waitingPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            waitingPanel.interactable = true;
            waitingPanel.blocksRaycasts = true;
        });
    }
}
