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
        db.GetGame(date,code);
    }
}
