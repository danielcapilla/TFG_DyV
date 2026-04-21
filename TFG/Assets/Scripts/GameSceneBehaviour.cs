using UnityEngine;

/// <summary>
/// Contiene los elementos de escena de un equipo y sus posiciones de spawn.
/// PlayerSpawner lee spawnPositions para colocar al jugador al entrar al juego.
/// </summary>
public class GameSceneBehaviour : MonoBehaviour
{
    [Tooltip("Posiciones donde aparecen los jugadores de este equipo.")]
    public Transform[] spawnPositions;

    private void Start()
    {
        // Auto-detectar posiciones de los hijos si no se asignaron en el inspector
        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            spawnPositions = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                spawnPositions[i] = transform.GetChild(i);
        }
    }
}
