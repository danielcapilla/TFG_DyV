using UnityEngine;

/// <summary>
/// Genera una cuadricula de TileSlots en la escena.
/// Configura el tamaño y el espaciado desde el inspector y pulsa
/// Generate Grid en el menu contextual o desde el Editor.
/// </summary>
public class GridGenerator : MonoBehaviour
{
    [Header("Tamaño de la cuadricula")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 4;

    [Header("Espaciado entre huecos")]
    [SerializeField] private float cellSize = 1.1f;

    [Header("Prefab del hueco")]
    [SerializeField] private GameObject tileSlotPrefab;

    /// <summary>
    /// Elimina los huecos existentes y genera la cuadricula de nuevo.
    /// Llamado desde el editor o en tiempo de ejecucion.
    /// </summary>
    public void GenerateGrid()
    {
        // Destruir huecos anteriores (hijos de este GameObject)
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        // Centrar la cuadricula en el transform de este objeto
        float offsetX = (columns - 1) * cellSize * 0.5f;
        float offsetZ = (rows - 1) * cellSize * 0.5f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 localPos = new Vector3(
                    col * cellSize - offsetX,
                    0f,
                    row * cellSize - offsetZ);

                GameObject slot;
                if (tileSlotPrefab != null)
                    slot = Instantiate(tileSlotPrefab, transform);
                else
                {
                    // Fallback: cubo plano como hueco visual
                    slot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    slot.transform.SetParent(transform);
                    slot.transform.localScale = new Vector3(cellSize * 0.95f, 0.05f, cellSize * 0.95f);
                    if (slot.GetComponent<TileSlot>() == null)
                        slot.AddComponent<TileSlot>();
                }

                slot.transform.localPosition = localPos;
                slot.name = $"Slot [{col},{row}]";
            }
        }

        Debug.Log($"GridGenerator: generada cuadricula {columns}x{rows}.");
    }

    // Genera la cuadricula automaticamente al entrar en Play si no tiene hijos
    private void Start()
    {
        if (transform.childCount == 0)
            GenerateGrid();
    }
}
