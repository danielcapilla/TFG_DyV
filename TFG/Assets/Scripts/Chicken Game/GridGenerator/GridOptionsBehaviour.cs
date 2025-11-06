using UnityEngine;
using UnityEngine.UI;

public class GridOptionsBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GridLevelGenerator gridLevelGenerator;
    [SerializeField] private GameObject gridOptionsPanel;
    [SerializeField] private Slider obstacleSlider;
    [SerializeField] private CameraController cameraController;

    public void Generate()
    {
        gridOptionsPanel.SetActive(false);
        gridLevelGenerator.GenerateLevel();
        cameraController.FitCameraToLevel();   
    }

    public void ObstaclesSlider()
    {
        gridLevelGenerator.obstacleDensity = obstacleSlider.value;
    }
}
