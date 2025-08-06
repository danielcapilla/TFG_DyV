using System.Collections;
using System.Collections.Generic;
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
    //Button background image?
}
