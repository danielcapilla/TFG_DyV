using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private PipeGameConfigSO config;

    [Header("Espaciado")]
    [SerializeField] private float cellSize = 1.1f;
    [SerializeField] private float reserveGap = 2f;
    [SerializeField] private int reserveSlotsPerColumn = 4;

    [Header("Prefab")]
    [SerializeField] private GameObject tileSlotPrefab;

    public float CellSize   => cellSize;
    public int   Columns    => config.columns;
    public int   Rows       => config.rows;

    public TileSlot[,] Slots        { get; private set; }
    public TileSlot[]  ReserveSlots { get; private set; }

    public void GenerateGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        int cols    = config.columns;
        int rows    = config.rows;
        int reserve = config.reserveSlots;

        Slots        = new TileSlot[cols, rows];
        ReserveSlots = new TileSlot[reserve];

        float offsetX = (cols - 1) * cellSize * 0.5f;
        float offsetZ = (rows - 1) * cellSize * 0.5f;

        // ── Grid principal ────────────────────────────────────────────────────
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 localPos = new Vector3(
                    col * cellSize - offsetX,
                    0f,
                    row * cellSize - offsetZ);

                TileSlot slot = SpawnSlot(localPos, $"Slot [{col},{row}]");
                Slots[col, row] = slot;
            }
        }

        // ── Zona de reserva (multiples columnas) ─────────────────────────────
        int slotsPerCol  = Mathf.Max(1, reserveSlotsPerColumn);
        int numCols      = Mathf.CeilToInt((float)reserve / slotsPerCol);
        float reserveStartX = offsetX + cellSize + reserveGap;

        for (int i = 0; i < reserve; i++)
        {
            int col = i / slotsPerCol;
            int row = i % slotsPerCol;

            // Centrar verticalmente cada columna
            int slotsInThisCol = (col == numCols - 1 && reserve % slotsPerCol != 0)
                ? reserve % slotsPerCol
                : slotsPerCol;
            float colOffsetZ = (slotsInThisCol - 1) * cellSize * 0.5f;

            Vector3 localPos = new Vector3(
                reserveStartX + col * cellSize,
                0f,
                row * cellSize - colOffsetZ);

            TileSlot slot = SpawnSlot(localPos, $"Reserve [{i}]");
            ReserveSlots[i] = slot;
        }

        Debug.Log($"GridGenerator: {cols}x{rows} + {reserve} slots de reserva.");

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
        // Regenerar si no hay slots o si el tamaño no coincide con el SO
        int expectedSlots = config.columns * config.rows + config.reserveSlots;
        if (transform.childCount == 0 || transform.childCount != expectedSlots)
            GenerateGrid();
        else
            RebuildSlotMatrix();
    }

    public void RebuildSlotMatrix()
    {
        int cols    = config.columns;
        int rows    = config.rows;
        int reserve = config.reserveSlots;

        Slots        = new TileSlot[cols, rows];
        ReserveSlots = new TileSlot[reserve];

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
                    if (col >= 0 && col < cols && row >= 0 && row < rows)
                        Slots[col, row] = slot;
            }
            else if (name.StartsWith("Reserve ["))
            {
                int bracket = name.IndexOf('[');
                int end     = name.IndexOf(']');
                if (bracket < 0 || end < 0) continue;
                if (int.TryParse(name.Substring(bracket + 1, end - bracket - 1), out int idx))
                    if (idx >= 0 && idx < reserve)
                        ReserveSlots[idx] = slot;
            }
        }
    }
}
