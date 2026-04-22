using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Evalua si hay un camino continuo de PipeTiles desde el PipeGenerator hasta el PipeReceiver.
///
/// Arquitectura de posiciones:
///   - Generator y Receiver son GameObjects FUERA o EN EL BORDE de la cuadricula.
///   - El BFS arranca desde la celda adyacente al generator en su OutputDirection.
///   - El circuito se completa cuando el BFS alcanza la celda adyacente al receiver
///     por el lado opuesto a su InputDirection.
///
/// Funciona tanto en offline como en online (en online solo el servidor cambia estado).
/// </summary>
public class PipeConnectionChecker : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PipeGenerator generator;
    [SerializeField] private PipeReceiver  receiver;
    [SerializeField] private GridGenerator grid;

    private Dictionary<Vector2Int, TileSlot> slotMap = new Dictionary<Vector2Int, TileSlot>();
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Start() => BuildSlotMap();

    // ── Mapa de slots ─────────────────────────────────────────────────────────

    public void BuildSlotMap()
    {
        slotMap.Clear();
        TileSlot[] slots = grid.GetComponentsInChildren<TileSlot>();
        float cell = grid.CellSize;
        foreach (var slot in slots)
        {
            Vector3 local = grid.transform.InverseTransformPoint(slot.transform.position);
            int col = Mathf.RoundToInt(local.x / cell);
            int row = Mathf.RoundToInt(local.z / cell);
            slotMap[new Vector2Int(col, row)] = slot;
        }
        Debug.LogWarning($"[PipeChecker] BuildSlotMap: {slotMap.Count} slots indexados");
    }

    // ── Evaluacion ────────────────────────────────────────────────────────────

    public void EvaluateCircuit()
    {
        // Offline: evaluar siempre. Online: solo el servidor cambia estado.
        if (!IsOffline && !IsServer) return;

        bool connected = FindPath();

        Debug.LogWarning($"[PipeChecker] EvaluateCircuit: connected={connected}");

        generator.SetConnected(connected, IsOffline);
        receiver.SetConnected(connected, IsOffline);
    }

    private bool FindPath()
    {
        float cell = grid.CellSize;

        // Celda de inicio: adyacente al generator en su direccion de salida
        Vector2Int genCell = WorldToGrid(generator.transform.position);
        Vector2Int startCell = genCell + DirToOffset(generator.OutputDirection);

        // Celda de llegada: adyacente al receiver por el lado opuesto a su InputDirection
        // El receiver mira hacia la cuadricula con su InputDirection,
        // asi que la ultima loseta debe estar en la celda adyacente al receiver
        // en la direccion opuesta a InputDirection
        Vector2Int recCell = WorldToGrid(receiver.transform.position);
        // targetCell: celda adyacente al receiver en la direccion de su InputDirection
        // (la loseta que alimenta al receiver esta en esa direccion)
        Vector2Int targetCell = recCell + DirToOffset(receiver.InputDirection);
        // La loseta en targetCell debe tener apertura apuntando AL receiver,
        // es decir en la direccion OPUESTA al InputDirection
        TileDirection requiredExitDir = TileShapeData.Opposite(receiver.InputDirection);

        Debug.LogWarning($"[PipeChecker] genCell={genCell} startCell={startCell} recCell={recCell} targetCell={targetCell}");
        Debug.LogWarning($"[PipeChecker] generator.OutputDir={generator.OutputDirection} receiver.InputDir={receiver.InputDirection}");
        Debug.LogWarning($"[PipeChecker] slotMap tiene startCell={slotMap.ContainsKey(startCell)}, targetCell={slotMap.ContainsKey(targetCell)}");

        if (!slotMap.ContainsKey(startCell)) return false;

        // BFS
        var visited = new HashSet<Vector2Int>();
        var queue   = new Queue<(Vector2Int cell, TileDirection enteredFrom)>();
        // Entramos al startCell viniendo desde el generator (opuesto a OutputDirection)
        queue.Enqueue((startCell, TileShapeData.Opposite(generator.OutputDirection)));

        while (queue.Count > 0)
        {
            var (pos, enteredFrom) = queue.Dequeue();
            if (visited.Contains(pos)) continue;
            visited.Add(pos);

            if (!slotMap.TryGetValue(pos, out TileSlot slot)) continue;
            PipeTile tile = slot.PlacedTile;
            if (tile == null) continue;

            // La loseta debe tener apertura por donde llegamos
            if (!tile.HasOpening(enteredFrom)) continue;

            Debug.LogWarning($"[PipeChecker] BFS en {pos}, enteredFrom={enteredFrom}, openings={tile.Openings}");

            // Comprobar si esta celda es la celda objetivo y tiene apertura hacia el receiver
            if (pos == targetCell && tile.HasOpening(requiredExitDir))
            {
                Debug.LogWarning($"[PipeChecker] Circuito completado!");
                return true;
            }

            // Propagar a vecinos
            foreach (TileDirection dir in new[]
                { TileDirection.North, TileDirection.South, TileDirection.East, TileDirection.West })
            {
                if (dir == enteredFrom) continue;
                if (!tile.HasOpening(dir)) continue;
                Vector2Int next = pos + DirToOffset(dir);
                if (!visited.Contains(next))
                    queue.Enqueue((next, TileShapeData.Opposite(dir)));
            }
        }
        return false;
    }

    // ── Utilidades ────────────────────────────────────────────────────────────

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float cell = grid.CellSize;
        Vector3 local = grid.transform.InverseTransformPoint(worldPos);
        return new Vector2Int(Mathf.RoundToInt(local.x / cell), Mathf.RoundToInt(local.z / cell));
    }

    private Vector2Int DirToOffset(TileDirection d)
    {
        switch (d)
        {
            case TileDirection.North: return new Vector2Int( 0,  1);
            case TileDirection.South: return new Vector2Int( 0, -1);
            case TileDirection.East:  return new Vector2Int( 1,  0);
            case TileDirection.West:  return new Vector2Int(-1,  0);
            default: return Vector2Int.zero;
        }
    }
}
