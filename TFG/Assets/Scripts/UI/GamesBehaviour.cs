using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private void OnEnable()
    {
        if (filters.date == "")
        {
            texto.text = "Código: " + filters.code;
        }
        else if (filters.code == "")
        {
            texto.text = "Fecha: " + filters.date;
        }
        else
        {
            texto.text = "Fecha: " + filters.date + "   Código: " + filters.code;
        }
        StartCoroutine(GetGames());
    }
    private void OnDisable()
    {
        filters.ClearData();
        foreach (Transform child in gamesGLG.transform)
        {
            Destroy(child.gameObject);
        }
    }
    IEnumerator GetGames()
    {
        yield return new WaitForSeconds(1);
        ShowGames();
    }
    private void ShowGames()
    {
        int i = 1;
        foreach (var data in filters.gameResponse.data)
        {
            GameObject partidaPrefab = Instantiate(partidaTarjetita, gamesGLG.transform);
            partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            i++;
        }

    }

}
