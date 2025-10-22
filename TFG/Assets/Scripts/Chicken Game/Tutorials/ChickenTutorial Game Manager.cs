using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChickenTutorialGameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [Header("Música")]
    [SerializeField] private AudioSource chickenMusic;
    [SerializeField] private AudioSource winMusic;
    [SerializeField] private AudioSource collisionSound;
    private PlayerInputController playerInputController;

    public Action<PlayerInputController> OnPlayerSpawned;
    public Action OnPlayerReachedGoal;

    private void Start()
    {
        SpawnPlayer();
        playerInputController.OnObstaculeCollided += PlayCollisionSound;    
        chickenMusic.Play();
    }
    private void OnDestroy()
    {
        playerInputController.OnObstaculeCollided -= PlayCollisionSound;
    }

    private void PlayCollisionSound(int obj)
    {
        collisionSound.Play();
    }

    private void SpawnPlayer()
    {
        Vector2Int startCell = GridLevelGenerator.Instance.start;

        // Calcular posicion en mundo segun la  grid
        Vector3 origin = GridLevelGenerator.Instance.transform.position - new Vector3((GridLevelGenerator.Instance.width - 1) * 0.5f * GridLevelGenerator.Instance.cellSize, 0f,
            (GridLevelGenerator.Instance.height - 1) * 0.5f * GridLevelGenerator.Instance.cellSize);

        Vector3 spawnXZ = origin + new Vector3(startCell.x * GridLevelGenerator.Instance.cellSize, 10f, startCell.y * GridLevelGenerator.Instance.cellSize); // Y=10 para raycast desde arriba

        // Raycast hacia abajo para encontrar el suelo
        RaycastHit hit;
        float spawnY = 0.5f; // valor por defecto si no hay suelo
        if (Physics.Raycast(spawnXZ, Vector3.down, out hit, 20f, LayerMask.GetMask("Default", "Ground")))
        {
            spawnY = hit.point.y + 0.1f; // 0.1f para evitar quedarse dentro del suelo
        }
        Vector3 spawnPosition = new Vector3(spawnXZ.x, spawnY, spawnXZ.z);

        // Instanciar player como NetworkObject
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        PlayerInputController playerInputController = player.GetComponent<PlayerInputController>();
        playerInputController.isTutorialMode = true;    
        player.GetComponent<PlayerInput>().enabled = true;
        this.playerInputController = playerInputController;
        OnPlayerSpawned?.Invoke(playerInputController);

    }
}
