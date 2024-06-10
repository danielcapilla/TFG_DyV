using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroupsBehaviour : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMP;
    [SerializeField]
    private GamesBehaviour gamesBehaviour;
    [SerializeField]
    private FiltersBehaviour filtersBehaviour;
    [SerializeField]
    private GameObject grupoTarjetita;
    [SerializeField]
    private GameObject groupsGLG;
    [SerializeField]
    private GameObject groupsGO;
    [SerializeField]
    private GameObject infoGO;
    private bool groupSelected = false;

    private void OnEnable()
    {
        textMP.text = "Partida: " + gamesBehaviour.gameCode;
        ShowGroups();
    }
    private void ShowGroups()
    {
        if(groupSelected) return;
        foreach (var data in filtersBehaviour.match.Equipos)
        {
            GameObject partidaPrefab = Instantiate(grupoTarjetita, groupsGLG.transform);
            partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = data.ID;
            GamePrefabScript gamePrefabScript = partidaPrefab.GetComponent<GamePrefabScript>();
            if (gamePrefabScript != null)
            {
                gamePrefabScript.SetObjectToActivate(infoGO);
                gamePrefabScript.SetObjectToDesactivate(groupsGO);
                gamePrefabScript.onClicked += ChangeBool;
            }
        }
    }
    private void OnDisable()
    {
        if (infoGO.activeInHierarchy) return;
        foreach (Transform child in groupsGLG.transform)
        {
            Destroy(child.gameObject);
        }
        groupSelected = false;
    }

    private void ChangeBool(object sender, string e)
    {
        groupSelected = true;
    }
}
