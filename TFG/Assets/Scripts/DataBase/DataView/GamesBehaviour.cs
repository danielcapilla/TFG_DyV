using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class GamesBehaviour : MonoBehaviour
{
    [SerializeField]
    private FiltersBehaviour filters;
    [SerializeField]
    private TextMeshProUGUI texto;
    [SerializeField]
    private GameObject gamesGLG;
    [SerializeField]
    private GameObject partidaTarjetita;
    [SerializeField]
    private GameObject groupsGO;
    [SerializeField]
    private GameObject gamesGO;
    private bool selectedGame = false;
    public string gameCode;
    private LocalizeStringEvent localizeStringEvent;
    private void OnEnable()
    {
        localizeStringEvent = texto.GetComponent<LocalizeStringEvent>();
        var fechaVar = localizeStringEvent.StringReference["fechaVariable"] as StringVariable;
        fechaVar.Value = filters.date;
        var codeVar = localizeStringEvent.StringReference["codVariable"] as StringVariable;
        codeVar.Value = filters.code;
        
        StartCoroutine(GetGames());
    }
    private void OnDisable()
    {
        if (groupsGO.activeInHierarchy) return;
        filters.ClearData();
        foreach (Transform child in gamesGLG.transform)
        {
            Destroy(child.gameObject);
        }
        selectedGame = false;
    }
    IEnumerator GetGames()
    {
        float t = 0f;
        while (filters.GetGamesCount() == 0 && t < 2f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        ShowGames();
    }
    private void ShowGames()
    {
        if (selectedGame) return;
        int count = filters.GetGamesCount();
        for (int i = 1; i <= count; i++)
        {
            GameObject partidaPrefab = Instantiate(partidaTarjetita, gamesGLG.transform);
            partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();

            GamePrefabScript gamePrefabScript = partidaPrefab.GetComponent<GamePrefabScript>();
            if (gamePrefabScript != null)
            {
                gamePrefabScript.SetObjectToActivate(groupsGO);
                gamePrefabScript.SetObjectToDesactivate(gamesGO);
                gamePrefabScript.onClicked += ChangeBool;
            }
        }

    }

    private void ChangeBool(object sender, string e)
    {
        selectedGame = true;
        gameCode = e;
        filters.SetGame(int.Parse(e) - 1);
    }
    public int NextGame()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        return (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text))-1;
    }

}
