using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Dates;
using UnityEngine;

public class FiltersBehaviour : MonoBehaviour
{
    [SerializeField]
    private DataBaseCommander db;
    [SerializeField]
    private DatePicker datePicker;
    [SerializeField]
    private TextMeshProUGUI codeText;
    public string date;
    public string code;
    public DataBaseCommander.GameResponse gameResponse;
    public void Filter()
    {
        code = codeText.text.Substring(0, codeText.text.Length - 1);
        try
        {
            date = datePicker.SelectedDate.Date.ToString("yyyy-MM-dd");
            
        }
        catch (System.Exception)
        {
            date = "";
        }
        db.GetGame(GetGames,date,code);
    }
    private void OnDisable()
    {
        Debug.Log("OnDisable");
        codeText.text = "";

    }
    public void GetGames(DataBaseCommander.GameResponse data)
    {
        gameResponse = data;
        Debug.Log(gameResponse.data.Count);
    }

    public void ClearData()
    {
        gameResponse.data.Clear();
    }
}
