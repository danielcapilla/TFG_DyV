using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseChickenTutorial : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    public void OnPauseButtonClick()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; 
    }
    public void OnResumeButtonClick()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; 
    }
    public void OnQuitButtonClick()
    {
        Time.timeScale = 1f;
        Destroy(FindFirstObjectByType<PlayerInputController>().gameObject);
        MinigameSelectorBehaviour.Instance.ReturnFromTutorial();

    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        var player = FindFirstObjectByType<PlayerInputController>();
        if (player != null)
        {
            Destroy(player.gameObject);
        }
    }
}
