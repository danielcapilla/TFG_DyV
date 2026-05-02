using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelectorBehaviour : NetworkBehaviour
{
    [Header("Panel selector")]
    [SerializeField] private GameObject minigameSelectorPanel;
    [SerializeField] private float time;
    [SerializeField] private Vector3 startPos;

    [Header("Referencias")]
    [SerializeField] private GameObject lobbyCamera;
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private Image gameImage;
    [SerializeField] private GameObject PlayButton;
    [SerializeField] private GameObject transparentPanel;

    [Header("Configuracion")]
    [SerializeField] private Transform configPanelContainer;
    [SerializeField] private GameObject configPanel;

    [Header("Textos")]
    [SerializeField] private LocalizeStringEvent DescriptionText;
    [SerializeField] private LocalizeStringEvent TutorialText;

    private string selectedGameScene;
    private string selectedTutorialScene;
    private Button selectedButton;
    private GameObject currentConfigPanel;
    private Scene loadedTutorialScene;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    public static MinigameSelectorBehaviour Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        startPos = minigameSelectorPanel.transform.localPosition;
    }

    // ── Panel selector ────────────────────────────────────────────────────────

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

    // ── Panel configuracion ───────────────────────────────────────────────────

    public void OpenConfigPanel()
    {
        if (configPanel != null)
            configPanel.SetActive(true);
    }

    public void CloseConfigPanel()
    {
        if (configPanel != null)
            configPanel.SetActive(false);
    }

    // ── Seleccion de juego ────────────────────────────────────────────────────

    public void SelectGame(MinigameInfoSO info)
    {
        // Actualizar UI
        gameImage.sprite = info.icon;
        DescriptionText.StringReference.SetReference(
            info.description.TableReference, info.description.TableEntryReference);
        TutorialText.StringReference.SetReference(
            info.tutorialText.TableReference, info.tutorialText.TableEntryReference);
        DescriptionText.RefreshString();
        TutorialText.RefreshString();

        selectedGameScene     = info.sceneName;
        selectedTutorialScene = info.tutorialSceneName;

        // Destruir panel de configuracion anterior e instanciar el nuevo
        if (currentConfigPanel != null)
        {
            Destroy(currentConfigPanel);
            currentConfigPanel = null;
            configPanel = null;
        }

        if (info.configPanelPrefab != null && configPanelContainer != null)
        {
            currentConfigPanel = Instantiate(info.configPanelPrefab, configPanelContainer);
            currentConfigPanel.SetActive(false);

            if (currentConfigPanel.TryGetComponent<IConfigPanel>(out var panel))
                panel.Setup(info.config);

            configPanel = currentConfigPanel;
        }
    }

    public void SelectButton(Button pressedButton)
    {
        if (selectedButton != null)
            selectedButton.interactable = true;
        selectedButton = pressedButton;
        selectedButton.interactable = false;
    }

    // ── Ir al juego ───────────────────────────────────────────────────────────

    public void IrAJuego()
    {
        if (string.IsNullOrEmpty(selectedGameScene)) return;

        if (IsOffline)
        {
            SceneManager.LoadScene(selectedGameScene, LoadSceneMode.Single);
            return;
        }

        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene(selectedGameScene, LoadSceneMode.Single);
    }

    // ── Tutorial ──────────────────────────────────────────────────────────────

    public void IrAlTutorial()
    {
        if (string.IsNullOrEmpty(selectedTutorialScene)) return;

        HideLobbyUI();
        var op = SceneManager.LoadSceneAsync(selectedTutorialScene, LoadSceneMode.Additive);
        op.completed += _ =>
        {
            loadedTutorialScene = SceneManager.GetSceneByName(selectedTutorialScene);
            if (loadedTutorialScene.IsValid())
                SceneManager.SetActiveScene(loadedTutorialScene);
        };
    }

    public void ReturnFromTutorial()
    {
        if (loadedTutorialScene.IsValid() && loadedTutorialScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(loadedTutorialScene).completed += _ =>
            {
                loadedTutorialScene = default;
                ShowLobbyUI();
            };
        }
        else
        {
            ShowLobbyUI();
        }
    }

    private void HideLobbyUI()
    {
        if (lobbyCanvas != null) lobbyCanvas.SetActive(false);
        if (lobbyCamera != null) lobbyCamera.SetActive(false);
    }

    private void ShowLobbyUI()
    {
        if (lobbyCanvas != null) lobbyCanvas.SetActive(true);
        if (lobbyCamera != null) lobbyCamera.SetActive(true);
    }
}
