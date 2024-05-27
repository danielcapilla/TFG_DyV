using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticPanelScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI groupName;
    [SerializeField]
    private TextMeshProUGUI score;

    public void SetData(string score)
    {
        this.score.text = score;
    }
}
