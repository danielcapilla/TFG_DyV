using UnityEngine;

public class PlayerGenerator : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    GridLevelGenerator levelGenerator;
    void Start()
    {
        levelGenerator = FindObjectOfType<GridLevelGenerator>();

        Vector2Int startCell = levelGenerator.start;
        Vector3 origin = levelGenerator.transform.position
                         - new Vector3((levelGenerator.width - 1) * 0.5f * levelGenerator.cellSize, 0f,
                                       (levelGenerator.height - 1) * 0.5f * levelGenerator.cellSize);
        Vector3 spawnPosition = origin + new Vector3(startCell.x * levelGenerator.cellSize, 0.5f, startCell.y * levelGenerator.cellSize);

        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
