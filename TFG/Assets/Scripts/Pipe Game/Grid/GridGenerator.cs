using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Cuadricula principal")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows    = 4;
    [SerializeField] private float cellSize = 1.1f;
    [SerializeField] private GameObject tileSlotPrefab;

    [Header("Zona de reserva (tiles eliminadas del camino)")]
    [SerializeField] private int reserveSlots = 6;
    [SerializeField] private float reserveGap = 2f; // separacion extra entre grid y reserva

    public float CellSize  => cellSize;
    public int   Columns   => columns;
    public int   Rows      => rows;

    // Matriz principal [col, row]
    public TileSlot[,] Slots { get; private set; }

    // Slots de reserva donde se colocan las tiles de los huecos
    public TileSlot[] ReserveSlots { get; private set; }

    public void GenerateGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        Slots        = new TileSlot[columns, rows];
        ReserveSlots = new TileSlot[reserveSlots];

        float offsetX = (columns - 1) * cellSize * 0.5f;
        float offsetZ = (rows    - 1) * cellSize * 0.5f;

        // ── Grid principal ────────────────────────────────────────────────────
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 localPos = new Vector3(
                    col * cellSize - offsetX,
                    0f,
                    row * cellSize - offsetZ);

                TileSlot slot = SpawnSlot(localPos, $"Slot [{col},{row}]");
                Slots[col, row] = slot;
            }
        }

        // ── Zona de reserva (a la derecha del grid) ───────────────────────────
        // Empieza justo despues del borde derecho del grid + reserveGap
        float reserveStartX = offsetX + cellSize + reserveGap;
        // Centrada verticalmente respecto al grid
        float reserveTotalZ = (reserveSlots - 1) * cellSize;
        float reserveOffsetZ = reserveTotalZ * 0.5f;

        for (int i = 0; i < reserveSlots; i++)
        {
            Vector3 localPos = new Vector3(
                reserveStartX,
                0f,
                i * cellSize - reserveOffsetZ);

            TileSlot slot = SpawnSlot(localPos, $"Reserve [{i}]");
            ReserveSlots[i] = slot;
        }

        Debug.Log($"GridGenerator: {columns}x{rows} + {reserveSlots} slots de reserva.");

        PipeConnectionChecker checker = FindFirstObjectByType<PipeConnectionChecker>();
        checker?.BuildSlotMap();
    }

    private TileSlot SpawnSlot(Vector3 localPos, string slotName)
    {
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
        slotGO.name = slotName;
        return slotGO.GetComponent<TileSlot>();
    }

    private void Awake()
    {
        if (transform.childCount == 0)
            GenerateGrid();
        else
            RebuildSlotMatrix();
    }

    public void RebuildSlotMatrix()
    {
        Slots        = new TileSlot[columns, rows];
        ReserveSlots = new TileSlot[reserveSlots];

        foreach (Transform child in transform)
        {
            TileSlot slot = child.GetComponent<TileSlot>();
            if (slot == null) continue;

            string name = child.name;

            if (name.StartsWith("Slot ["))
            {
                int bracket = name.IndexOf('[');
                int comma   = name.IndexOf(',');
                int end     = name.IndexOf(']');
                if (bracket < 0 || comma < 0 || end < 0) continue;
                if (int.TryParse(name.Substring(bracket + 1, comma - bracket - 1), out int col) &&
                    int.TryParse(name.Substring(comma + 1, end - comma - 1), out int row))
                    if (col >= 0 && col < columns && row >= 0 && row < rows)
                        Slots[col, row] = slot;
            }
            else if (name.StartsWith("Reserve ["))
            {
                int bracket = name.IndexOf('[');
                int end     = name.IndexOf(']');
                if (bracket < 0 || end < 0) continue;
                if (int.TryParse(name.Substring(bracket + 1, end - bracket - 1), out int idx))
                    if (idx >= 0 && idx < reserveSlots)
                        ReserveSlots[idx] = slot;
            }
        }
    }
}
