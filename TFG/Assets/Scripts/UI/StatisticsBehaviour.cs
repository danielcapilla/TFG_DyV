using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StatisticsBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject levv;

    private Transform child;

    private Transform child2;
    [SerializeField]
    private GameObject statisticPanel;
    [SerializeField]
    private TeamMenager teamManager;
    private LocalizeStringEvent localizeStringEvent;
    [SerializeField]
    private GameObject hamburger;
    private bool positionchanged = true;
    public bool finished { get; private set; }
    private void Awake()
    {
        finished = true;
    }
    private void Start()
    {
        for (int i = 0; i < teamManager.teams.Count; i++)
        {
            GameObject instance = Instantiate(statisticPanel);
            instance.transform.SetParent(levv.transform);
            StatisticPanelScript statisticPanelScript = instance.GetComponent<StatisticPanelScript>();
            localizeStringEvent = instance.GetComponentInChildren<LocalizeStringEvent>();
            var groupIdLocalizationString = localizeStringEvent.StringReference["groupId"] as IntVariable;
            groupIdLocalizationString.Value = teamManager.teams[i].ID;
            statisticPanelScript.SetData( $"{teamManager.teams[i].Puntuacion}");
        }
    }
    public void TurnOffStatistics()
    {
        if (positionchanged)
        {
            levv.SetActive(false);
            hamburger.SetActive(true);
            positionchanged = false;
        }
        else
        {
            levv.SetActive(true);
            hamburger.SetActive(false);
            positionchanged = true;
        }
    }
    public void ChangePosition(int aDondeVoy, int deDondeVengo, int id)
    {
        child = levv.transform.GetChild(deDondeVengo);
        child.gameObject.GetComponent<StatisticPanelScript>().SetData($"{teamManager.teams[id].Puntuacion}");
        if (aDondeVoy == deDondeVengo) return;
        finished = false;
        foreach (LayoutElement le in levv.transform.GetComponentsInChildren<LayoutElement>())
        {
            le.ignoreLayout = true;
        }
        child2 = levv.transform.GetChild(aDondeVoy);
        float newPosY = child2.position.y;
        child.DOMoveY(newPosY, 2f).SetEase(Ease.InOutSine).OnPlay(() =>
        {
            for (int i = deDondeVengo - 1; i >= aDondeVoy; i--)
            {
                levv.transform.GetChild(i).DOMoveY(levv.transform.GetChild(i + 1).position.y, 2f).SetEase(Ease.InOutSine);
            }
        }).OnComplete(() => {
            child.transform.SetSiblingIndex(aDondeVoy);
            foreach (LayoutElement le in levv.transform.GetComponentsInChildren<LayoutElement>())
            {
                le.ignoreLayout = false;
            }
            teamManager.UpdateIndex(deDondeVengo,aDondeVoy);
            finished = true;
        });
    }
}
