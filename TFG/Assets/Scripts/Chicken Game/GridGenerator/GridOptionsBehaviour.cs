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

    [SerializeField] private CameraController cameraController;

    private void Start()
    {
        ObstaclesSlider();
        DistanceSlider();
        HeightSlider();
        WidthSlider();

        startXInputField.onValueChanged.AddListener(OnStartChanged);
        startYInputField.onValueChanged.AddListener(OnStartChanged);

        UpdateStartLimits();
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
}
