using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Tamaño de la cuadricula")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows    = 4;

    [Header("Espaciado entre huecos")]
    [SerializeField] private float cellSize = 1.1f;

    [Header("Prefab del hueco")]
    [SerializeField] private GameObject tileSlotPrefab;

    public float CellSize => cellSize;
    public int   Columns  => columns;
    public int   Rows     => rows;

    // Matriz [col, row] de TileSlots generada al crear el grid
    public TileSlot[,] Slots { get; private set; }

    public void GenerateGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        Slots = new TileSlot[columns, rows];

        float offsetX = (columns - 1) * cellSize * 0.5f;
        float offsetZ = (rows    - 1) * cellSize * 0.5f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 localPos = new Vector3(
                    col * cellSize - offsetX,
                    0f,
                    row * cellSize - offsetZ);

                GameObject slotGO;
                if (tileSlotPrefab != null)
                    slotGO = Instantiate(tileSlotPrefab, transform);
                else
                {
                    slotGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    slotGO.transform.SetParent(transform);
                    slotGO.transform.localScale = new Vector3(cellSize * 0.95f, 0.05f, cellSize * 0.95f);
                    if (slotGO.GetComponent<TileSlot>() == null)
                        slotGO.AddComponent<TileSlot>();
                }

                slotGO.transform.localPosition = localPos;
                slotGO.name = $"Slot [{col},{row}]";
                Slots[col, row] = slotGO.GetComponent<TileSlot>();
            }
        }

        Debug.Log($"GridGenerator: cuadricula {columns}x{rows} generada.");

        PipeConnectionChecker checker = FindFirstObjectByType<PipeConnectionChecker>();
        checker?.BuildSlotMap();
    }

    private void Awake()
    {
        if (transform.childCount == 0)
            GenerateGrid();
        else
            RebuildSlotMatrix();
    }

    // Reconstruye la matriz a partir de los hijos existentes (por si ya estaba generado)
    public void RebuildSlotMatrix()
    {
        Slots = new TileSlot[columns, rows];
        foreach (Transform child in transform)
        {
            TileSlot slot = child.GetComponent<TileSlot>();
            if (slot == null) continue;
            // El nombre es "Slot [col,row]"
            string name = child.name;
            int bracket = name.IndexOf('[');
            int comma   = name.IndexOf(',');
            int end     = name.IndexOf(']');
            if (bracket < 0 || comma < 0 || end < 0) continue;
            if (int.TryParse(name.Substring(bracket + 1, comma - bracket - 1), out int col) &&
                int.TryParse(name.Substring(comma + 1, end - comma - 1), out int row))
            {
                if (col >= 0 && col < columns && row >= 0 && row < rows)
                    Slots[col, row] = slot;
            }
        }
    }
}
