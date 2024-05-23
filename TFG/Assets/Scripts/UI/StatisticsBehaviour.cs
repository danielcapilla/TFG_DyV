using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject levv;

    private Transform child;

    private Transform child2;



    public void Prueba()
    {
        foreach (LayoutElement le in levv.transform.GetComponentsInChildren<LayoutElement>())
        {
            le.ignoreLayout = true;
        }
        Debug.Log("StatisticsBehaviour Start");
        child = levv.transform.GetChild(1);
        child2 = levv.transform.GetChild(0);
        //child.GetComponent<LayoutElement>().ignoreLayout = true;
        child.DOMoveY(child2.position.y, 2f).SetEase(Ease.InOutSine).OnPlay(() =>
        {
            child2.DOMoveY(child.position.y, 2f).SetEase(Ease.InOutSine);
        }).OnComplete(() => {
            child.transform.SetSiblingIndex(0);
            //child2.transform.SetSiblingIndex(1);
            //child.GetComponent<LayoutElement>().ignoreLayout = false;
            foreach (LayoutElement le in levv.transform.GetComponentsInChildren<LayoutElement>())
            {
                le.ignoreLayout = false;
            }
        });
    }
}
