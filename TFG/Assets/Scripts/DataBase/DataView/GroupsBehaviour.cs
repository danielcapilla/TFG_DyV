using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

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
    private GameObject infoGOHamburger;
    [SerializeField]
    private GameObject infoGOChicken;
    [SerializeField]
    private GameObject infoGOPipe;

    private bool groupSelected = false;
    public string groupSelectedID;
    private LocalizeStringEvent localizeStringEvent;

    private void OnEnable()
    {
        localizeStringEvent = textMP.GetComponent<LocalizeStringEvent>();
        var gameVar = localizeStringEvent.StringReference["gameVariable"] as IntVariable;
        gameVar.Value = int.Parse(gamesBehaviour.gameCode);
        ShowGroups();
    }

    private void ShowGroups()
    {
        if (groupSelected) return;

        int i = 1;

        if (filtersBehaviour.selectedGameType == FiltersBehaviour.GameType.Restaurant)
        {
            foreach (var data in filtersBehaviour.match.Equipos)
            {
                GameObject partidaPrefab = Instantiate(grupoTarjetita, groupsGLG.transform);
                partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
                var gamePrefabScript = partidaPrefab.GetComponent<GamePrefabScript>();
                if (gamePrefabScript != null)
                {
                    gamePrefabScript.SetObjectToActivate(infoGOHamburger);
                    gamePrefabScript.SetObjectToDesactivate(groupsGO);
                    gamePrefabScript.onClicked += ChangeBool;
                }
                i++;
            }
        }
        else if (filtersBehaviour.selectedGameType == FiltersBehaviour.GameType.Pipe)
        {
            // Pipe: una tarjeta por grupo (de momento grupo 0 por defecto, preparado para mas)
            int groupCount = 1; // TODO: extraer grupos reales cuando haya multiples
            for (int g = 0; g < groupCount; g++)
            {
                int groupIndex = g;
                GameObject partidaPrefab = Instantiate(grupoTarjetita, groupsGLG.transform);
                partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = (g + 1).ToString();
                var gamePrefabScript = partidaPrefab.GetComponent<GamePrefabScript>();
                if (gamePrefabScript != null)
                {
                    gamePrefabScript.SetObjectToActivate(infoGOPipe);
                    gamePrefabScript.SetObjectToDesactivate(groupsGO);
                    gamePrefabScript.onClicked += (s, e) => { groupSelectedID = groupIndex.ToString(); ChangeBool(s, e); };
                }
                i++;
            }
        }
        else if(filtersBehaviour.selectedGameType == FiltersBehaviour.GameType.Chicken) // Chicken
        {
            var distinctGroups = filtersBehaviour.chickenMatch.Groups.Select(g => g.Group).Distinct().OrderBy(g => g).ToList();
            foreach (var data in distinctGroups)
            {
                GameObject partidaPrefab = Instantiate(grupoTarjetita, groupsGLG.transform);
                partidaPrefab.GetComponentInChildren<TextMeshProUGUI>().text = (data).ToString();
                var gamePrefabScript = partidaPrefab.GetComponent<GamePrefabScript>();
                if (gamePrefabScript != null)
                {
                    gamePrefabScript.SetObjectToActivate(infoGOChicken);
                    gamePrefabScript.SetObjectToDesactivate(groupsGO);
                    gamePrefabScript.onClicked += ChangeBool;
                }
                i++;
            }
        }
    }

    private void OnDisable()
    {
        if (infoGOChicken.activeInHierarchy || infoGOHamburger.activeInHierarchy) return;

        foreach (Transform child in groupsGLG.transform)
        {
            Destroy(child.gameObject);
        }
        groupSelected = false;
    }

    private void ChangeBool(object sender, string e)
    {
        groupSelected = true;
        groupSelectedID = (int.Parse(e) - 1).ToString();
    }
}