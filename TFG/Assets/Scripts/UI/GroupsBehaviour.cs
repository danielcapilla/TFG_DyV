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

    private void OnEnable()
    {
        textMP.text = "Partida: " + gamesBehaviour.gameCode;

    }
}
