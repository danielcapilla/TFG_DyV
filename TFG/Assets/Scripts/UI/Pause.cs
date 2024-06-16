using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : NetworkBehaviour
{
    [SerializeField]
    private Button pauseButton;
    [SerializeField]
    private Button resumeButton;
    [SerializeField]
    private Button exitButton;
    [SerializeField]
    private GameObject pauseMenuCanvas;
    [SerializeField]
    private TeamMenager teamMenager;
    [SerializeField]
    private GameObject pausePanel;

    public void PauseGame()
    {
        pauseButton.gameObject.SetActive(false);
        pauseMenuCanvas.SetActive(true);
        PauseGameClientRPC();
    }
    [ClientRpc]
    private void PauseGameClientRPC()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }
    public void ResumeGame()
    {
        pauseButton.gameObject.SetActive(true);
        pauseMenuCanvas.SetActive(false);
        ResumeGameClientRPC();
    }
    [ClientRpc]
    private void ResumeGameClientRPC()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }
    public void ExitGame()
    {
        ExitGameClientRPC();
        LateJoinsBehaviour.aprovedConection = true;
        Destroy(teamMenager.gameObject);
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
    [ClientRpc]
    private void ExitGameClientRPC()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(true);
        DOTween.Clear(true);
    }

}
