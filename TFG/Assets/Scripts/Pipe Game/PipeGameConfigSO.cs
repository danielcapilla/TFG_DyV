using UnityEngine;

[CreateAssetMenu(fileName = "PipeGameConfig", menuName = "Pipe Game/Config")]
public class PipeGameConfigSO : ScriptableObject
{
    [Header("Tamaño del grid")]
    public int columns     = 4;
    public int rows        = 4;
    public int reserveSlots = 6;

    [Header("Dificultad")]
    [Range(0.1f, 0.8f)] public float gapRatio  = 0.35f;
    [Range(0f,   1f)]   public float lockRatio  = 0.4f;
    [Range(0, 10)]      public int   extraTiles  = 3;
}
