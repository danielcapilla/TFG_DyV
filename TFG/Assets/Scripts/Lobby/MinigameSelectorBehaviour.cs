using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Localization.Components;

public class MinigameSelectorBehaviour : NetworkBehaviour
{
    [SerializeField] GameObject minigameSelectorPanel;
    [SerializeField] float time;
    [SerializeField] Vector3 startPos;

    [SerializeField] Image gameImage;

    [SerializeField] string selectedGame;
    [SerializeField] GameObject PlayButton;
    [SerializeField] GameObject transparentPanel;
    Button selectedButton;

    [SerializeField] LocalizeStringEvent DescriptionText;
    [SerializeField] LocalizeStringEvent TutorialText;

    // Start is called before the first frame update
    void Start()
    {
        startPos = minigameSelectorPanel.transform.localPosition;
        if (!IsServer) 
        {
            PlayButton.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        minigameSelectorPanel.transform.DOLocalMove(Vector3.zero, time);
        transparentPanel.SetActive(true);
    }

    public void ClosePanel() 
    {
        minigameSelectorPanel.transform.DOLocalMove(startPos, time);
        transparentPanel.SetActive(false);
    }

    public void IrAJuego()
    {
        //TODO If Host start game if client cast vote to poll
        if (selectedGame.Length > 0)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(selectedGame, LoadSceneMode.Single);
        }
    }

    public void SelectButton(Button pressedButton) 
    {
        if (selectedButton != null) 
        {
            selectedButton.interactable = true;
        }
        selectedButton = pressedButton;
        selectedButton.interactable = false;
    }

    public void SelectGame(MinigameInfoSO info) 
    {
        gameImage.sprite = info.icon;
        DescriptionText.StringReference.SetReference(info.description.TableReference, info.description.TableEntryReference);
        TutorialText.StringReference.SetReference(info.tutorialText.TableReference, info.tutorialText.TableEntryReference);
        DescriptionText.RefreshString();
        TutorialText.RefreshString();
        selectedGame = info.sceneName;
    }
}
