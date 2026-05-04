using UnityEngine;

[CreateAssetMenu(fileName = "PipeGameConfig", menuName = "Game Config/Pipe Game")]
public class PipeGameConfigSO : GameConfigBaseSO
{
    [Header("Grid")]
    [Range(4, 8)] public int columns      = 4;
    [Range(4, 8)] public int rows         = 4;
    public int reserveSlots = 6;

    [Header("Dificultad")]
    [HideInInspector] public string difficulty = "Normal";
    [Range(0.1f, 0.8f)] public float gapRatio  = 0.35f;
    [Range(0f,   1f)]   public float lockRatio  = 0.4f;
    [Range(0, 10)]      public int   extraTiles  = 3;
}
