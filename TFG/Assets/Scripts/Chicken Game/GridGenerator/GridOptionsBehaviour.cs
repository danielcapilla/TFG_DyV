using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GridOptionsBehaviour : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GridLevelGenerator gridLevelGenerator;
    [SerializeField] private GameObject gridOptionsPanel;
    [SerializeField] private Slider obstacleSlider;
    [SerializeField] private TextMeshProUGUI obstacleValueText;
    [SerializeField] private Slider distanceSlider;
    [SerializeField] private TextMeshProUGUI distanceValueText;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TextMeshProUGUI heightValueText;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private TextMeshProUGUI widthValueText;
    [SerializeField] private DraggableNumberField startXInputField;
    [SerializeField] private DraggableNumberField startYInputField;
    [SerializeField] private GameObject pauseButton;

    [SerializeField] private UIGradient gradient;
    [SerializeField] private Graphic graphic;
    [SerializeField] private RectTransform panelTransform;

    // Tweens registrados (para la limpieza)
    private Tweener gradientTween;
    private Tweener introScaleTween;

    [SerializeField] private CameraController cameraController;

    private void Start()
    {
        if(!IsServer)
        {
            gridOptionsPanel.SetActive(false);
            pauseButton.SetActive(false);
            return;
        }

        // Gradiente que respira
        if (gradient != null)
        {
            gradientTween = DOTween.To(
                () => gradient.m_color1,
                x => { gradient.m_color1 = x; if (graphic != null) graphic.SetAllDirty(); },
                new Color(1f, 0.7f, 0.3f),
                2f
            ).SetLoops(-1, LoopType.Yoyo);
        }
        // Animacion de entrada
        if (panelTransform != null)
        {
            panelTransform.localScale = Vector3.zero;
            introScaleTween = panelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);           
        }

        ObstaclesSlider();
        DistanceSlider();
        HeightSlider();
        WidthSlider();

        startXInputField.onValueChanged.AddListener(OnStartChanged);
        startYInputField.onValueChanged.AddListener(OnStartChanged);

        UpdateStartLimits();
    }
    void OnDisable()
    {
        KillAllTweens();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(!IsServer)
            return;
        KillAllTweens();
    }

    public void Generate()
    {
        gridOptionsPanel.SetActive(false);
        gridLevelGenerator.GenerateLevel();
        // Tiene que ser una RPC
        CameraControllerRPC();
        
    }
    [Rpc(SendTo.Everyone)]
    private void CameraControllerRPC()
    {
        cameraController.FitCameraToLevel();
    }
    public void ObstaclesSlider()
    {
        gridLevelGenerator.obstacleDensity = obstacleSlider.value;
        obstacleValueText.text = (Mathf.RoundToInt((obstacleSlider.value * 100))).ToString() + "%";
    }
    public void DistanceSlider()
    {
        gridLevelGenerator.minPathLength = (int)distanceSlider.value;
        distanceValueText.text = distanceSlider.value.ToString();
    }
    public void HeightSlider()
    {
        gridLevelGenerator.height = (int)heightSlider.value;
        heightValueText.text = heightSlider.value.ToString();

        UpdateStartLimits();
    }
    public void WidthSlider()
    {
        gridLevelGenerator.width = (int)widthSlider.value;
        widthValueText.text = widthSlider.value.ToString();

        UpdateStartLimits();
    }

    private void UpdateStartLimits()
    {
        int width = gridLevelGenerator.width;
        int height = gridLevelGenerator.height;
        // Limitar los valores de startX y startY al tamaño del grid
        startXInputField.MinValue = 0;
        startXInputField.MaxValue = width - 1;
        startYInputField.MinValue = 0;
        startYInputField.MaxValue = height - 1;

        startXInputField.SetValue(Mathf.Clamp(startXInputField.GetValue(), 0, width - 1));
        startYInputField.SetValue(Mathf.Clamp(startYInputField.GetValue(), 0, height - 1));

        gridLevelGenerator.start.x = startXInputField.GetValue();
        gridLevelGenerator.start.y = startYInputField.GetValue();

        UpdateDistanceSliderRange(gridLevelGenerator.start.x, gridLevelGenerator.start.y);
    }
    private void UpdateDistanceSliderRange(int startX, int startY)
    {
        int width = gridLevelGenerator.width;
        int height = gridLevelGenerator.height;
        // Calcular la distancia maxima posible desde el punto de inicio a cualquier esquina del grid
        int topLeft = startX + startY;
        int topRight = (width - 1 - startX) + startY;
        int bottomLeft = startX + (height - 1 - startY);
        int bottomRight = (width - 1 - startX) + (height - 1 - startY);

        int maxPossibleDistance = Mathf.Max(topLeft, topRight, bottomLeft, bottomRight);

        distanceSlider.minValue = 1;
        distanceSlider.maxValue = maxPossibleDistance;

        if (distanceSlider.value > maxPossibleDistance)
        {
            distanceSlider.value = maxPossibleDistance;
            DistanceSlider(); 
        }
    }
    private void OnStartChanged(int _)
    {
        // Actualizar la distancia puesto que el start ha cambiado
        gridLevelGenerator.start.x = startXInputField.GetValue();
        gridLevelGenerator.start.y = startYInputField.GetValue();

        UpdateDistanceSliderRange(gridLevelGenerator.start.x, gridLevelGenerator.start.y);
    }
    private void KillAllTweens()
    {
        gradientTween?.Kill();
        introScaleTween?.Kill();

        if (panelTransform) DOTween.Kill(panelTransform);
        if (gradient) DOTween.Kill(gradient);

    }
}
