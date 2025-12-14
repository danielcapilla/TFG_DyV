using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChickenTutorialGameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private GridLevelGenerator gridLevelGenerator;
    [SerializeField] private CameraController cameraController;
    [Header("Música")]
    [SerializeField] private AudioSource chickenMusic;
    [SerializeField] private AudioSource winMusic;
    [SerializeField] private AudioSource collisionSound;
    
    private PlayerInputController playerInputController;
    private int punctuation = 0;    

    public Action<PlayerInputController> OnPlayerSpawned;
    public Action OnPlayerReachedGoal;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "ChickenTutorial 1" || scene.name == "ChickenTutorial 2")
            Initialize();

    }
    private void OnSceneUnloaded(Scene scene)
    {
        PlayerInputController player = FindFirstObjectByType<PlayerInputController>();
        if (player != null)
        {
            Destroy(player.gameObject);
        }
    }
    private void Start()
    {
        //Initialize();
    }
    private void Awake()
    {
        gridLevelGenerator.GenerateTutorialLevel();
        FitCamera();
    }
    private void Initialize()
    {
        SpawnPlayer();
        playerInputController.OnObstaculeCollided += PlayCollisionSound;
        groupBehaviour.OnTurnExecuted += CalculatePunctuation;
        //chickenMusic.Play();
    }

    private void FitCamera()
    {
        cameraController.FitCameraToLevel();
    }

    private void OnDestroy()
    {

        playerInputController.OnObstaculeCollided -= PlayCollisionSound;
        groupBehaviour.OnTurnExecuted -= CalculatePunctuation;
        gridLevelGenerator.OnLevelGenerated -= FitCamera;
    }

    private void PlayCollisionSound(int obj)
    {
        if(collisionSound != null)
            collisionSound.Play();
    }
    private void CalculatePunctuation()
    {
        float progress = gridLevelGenerator.GetProgress(playerInputController.CurrentGridPos);
        punctuation = (int)(progress * 100f);
        if (punctuation == 100)
        {
            winMusic.Play();
            OnPlayerReachedGoal?.Invoke();
        }
    }
    private void SpawnPlayer()
    {
        Vector2Int startCell = gridLevelGenerator.start;

        Vector3 origin = gridLevelGenerator.transform.position - new Vector3((gridLevelGenerator.width - 1) * 0.5f * gridLevelGenerator.cellSize, 0f,
            (gridLevelGenerator.height - 1) * 0.5f * gridLevelGenerator.cellSize);

        Vector3 spawnXZ = origin + new Vector3(startCell.x * gridLevelGenerator.cellSize, 10f, startCell.y * gridLevelGenerator.cellSize);

        RaycastHit hit;
        float spawnY = 0.5f;
        if (Physics.Raycast(spawnXZ, Vector3.down, out hit, 20f, LayerMask.GetMask("Default", "Ground")))
        {
            spawnY = hit.point.y + 0.1f;
        }
        Vector3 spawnPosition = new Vector3(spawnXZ.x, spawnY, spawnXZ.z);

        // Reutilizar si ya existe un player en la escena para el tuto 2
        PlayerInputController existing = FindFirstObjectByType<PlayerInputController>();
        if (existing != null && existing.isTutorialMode)
        {
            this.playerInputController = existing;
            // Reposicionar 
            existing.transform.position = spawnPosition;
            existing.CurrentGridPos = startCell;
            existing.IsMoving = false;
            existing.LastMoveBlocked = false;
            existing.isTutorialMode = true;
            //DontDestroyOnLoad(existing.gameObject);

            OnPlayerSpawned?.Invoke(this.playerInputController);
            return;
        }

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        PlayerInputController playerInputController = player.GetComponent<PlayerInputController>();
        playerInputController.isTutorialMode = true;
        player.GetComponent<PlayerInput>().enabled = true;
        this.playerInputController = playerInputController;
        // Evitar que Unity lo destruya al cambiar de escena
        DontDestroyOnLoad(player);

        OnPlayerSpawned?.Invoke(playerInputController);
    }
}
