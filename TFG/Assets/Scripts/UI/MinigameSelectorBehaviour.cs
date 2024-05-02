using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MinigameSelectorBehaviour : NetworkBehaviour
{
    [SerializeField] GameObject minigameSelectorPanel;
    [SerializeField] float time;
    [SerializeField] Vector3 startPos;

    [SerializeField] TextMeshProUGUI GameDescription;
    [SerializeField] Image gameImage;

    [SerializeField] string selectedGame;
    [SerializeField] GameObject PlayButton;

    // Start is called before the first frame update
    void Start()
    {
        startPos = minigameSelectorPanel.transform.localPosition;
        if (IsClient) 
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
        minigameSelectorPanel.transform.DOLocalMove(Vector3.zero, time);
    }

    public void ClosePanel() 
    {
        minigameSelectorPanel.transform.DOLocalMove(startPos, time);
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
}
