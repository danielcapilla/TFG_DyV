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
    }
    public void ExitGame()
    {
        Time.timeScale = 1;
        LateJoinsBehaviour.aprovedConection = true;
        Destroy(teamMenager.gameObject);
        DOTween.Clear(true);
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

}
