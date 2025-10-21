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
    // For Tutorials
    [SerializeField] GameObject lobbyCamera;
    [SerializeField] GameObject lobbyCanvas;

    [SerializeField] Image gameImage;

    [SerializeField] string selectedGame;
    [SerializeField] GameObject PlayButton;
    [SerializeField] GameObject transparentPanel;
    Button selectedButton;

    [SerializeField] LocalizeStringEvent DescriptionText;
    [SerializeField] LocalizeStringEvent TutorialText;
    // Singleton
    public static MinigameSelectorBehaviour Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        startPos = minigameSelectorPanel.transform.localPosition;
        //if (!IsServer) 
        //{
        //    PlayButton.SetActive(false);
        //}
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
        if(IsServer)
        {
            if (selectedGame.Length > 0)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(selectedGame, LoadSceneMode.Single);
            }
        }
        else
        {
            if (selectedGame.Length > 0)
            {
                HideLobbyUI();
                SceneManager.LoadSceneAsync(selectedGame, LoadSceneMode.Additive);
            }
        }

    }
    private void HideLobbyUI()
    {
        lobbyCanvas.SetActive(false);
        if (lobbyCamera != null)
            lobbyCamera.gameObject.SetActive(false);
    }
    public void ReturnFromTutorial()
    {
        Scene tutorialScene = SceneManager.GetSceneByName(selectedGame);

        if (tutorialScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(tutorialScene).completed += (op) =>
            {
                lobbyCanvas.SetActive(true);
                if (lobbyCamera != null)
                    lobbyCamera.gameObject.SetActive(true);
            };
        }
        else
        {
            lobbyCanvas.SetActive(true);
            if (lobbyCamera != null)
                lobbyCamera.gameObject.SetActive(true);
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
        // New
        if(IsServer)
            selectedGame = info.sceneName;
        else
            selectedGame = info.tutorialSceneName;
    }
}
