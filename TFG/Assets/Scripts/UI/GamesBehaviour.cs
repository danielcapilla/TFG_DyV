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

    private void Start()
    {
        if(filters.date =="")
        {
            texto.text = "Código: " + filters.code;
        }
        else if(filters.code == "")
        {
            texto.text = "Fecha: " + filters.date;
        }
        else
        {
            texto.text ="Fecha: " + filters.date + "   Código: " + filters.code;
        }
        ShowGames();
    }

    private void ShowGames()
    {
        //Mostrar partidas de la info que se recibe e instanciar las partidas
    }
}
