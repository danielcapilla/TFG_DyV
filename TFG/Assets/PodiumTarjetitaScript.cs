using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class PodiumTarjetitaScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI position;
    [SerializeField]
    private TextMeshProUGUI groupName;
    [SerializeField]
    private TextMeshProUGUI score;

    public void SetData(string position, string score)
    {
        this.position.text = position;
        //this.groupName.text = groupName;
        this.score.text = score;
    }
}
