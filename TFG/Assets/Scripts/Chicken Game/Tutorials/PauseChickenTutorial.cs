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
        MinigameSelectorBehaviour.Instance.ReturnFromTutorial();

    }

}
