using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinigameSelectorBehaviour : NetworkBehaviour
{
    [SerializeField] GameObject minigameSelectorPanel;
    [SerializeField] float time;
    [SerializeField] Vector3 startPos;

    [SerializeField] Image gameImage;
    [SerializeField] List<Sprite> minigameImages;

    [SerializeField] string selectedGame;
    [SerializeField] GameObject PlayButton;
    [SerializeField] GameObject transparentPanel;
    Button selectedButton;

    // Start is called before the first frame update
    void Start()
    {
        startPos = minigameSelectorPanel.transform.localPosition;
        if (!IsServer) 
        {
            PlayButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPanel()
    {
        Debug.Log("OpenPanel");
        minigameSelectorPanel.transform.DOLocalMove(Vector3.zero, time);
        transparentPanel.SetActive(true);
    }

    public void ClosePanel() 
    {
        minigameSelectorPanel.transform.DOLocalMove(startPos, time);
        transparentPanel.SetActive(false);
    }

    public void SelectGame(string value) 
    {
        selectedGame = value;
    }

    public void IrAJuego()
    {
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

    public void ChangeImage(int id) 
    {
        gameImage.sprite = minigameImages[id];
    }
}
