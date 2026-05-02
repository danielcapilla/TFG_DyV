using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "MiniGames/MinigameInfo")]
public class MinigameInfoSO : ScriptableObject
{
    public LocalizedString gameName;
    public LocalizedString description;
    public LocalizedString tutorialText;
    public string sceneName;
    public Sprite icon;
    public string tutorialSceneName;

    [Header("Configuracion")]
    public GameConfigBaseSO config;
    public GameObject configPanelPrefab;
}
