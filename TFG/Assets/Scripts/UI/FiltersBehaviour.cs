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

    public BurguerJSONCreator.Match match;
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
        db.GetGame(GetGames, date, code);
    }
    private void OnDisable()
    {
        codeText.text = "";

    }
    public void GetGames(DataBaseCommander.GameResponse data)
    {
        gameResponse = data;
        match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[0].BurguersDelivered);
    }
    public void SetGame(int game)
    {
        match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[game].BurguersDelivered);
    }
    public void ClearData()
    {
        gameResponse.data.Clear();
    }

    public void ShowMatch()
    {
        foreach (List<BurguerJSONCreator.Ingredientes> burguer in match.HamburguesasEjemplo)
        {
            Debug.Log("Hamburguesa Ejemplo: " + match.HamburguesasEjemplo.IndexOf(burguer));
            foreach (BurguerJSONCreator.Ingredientes ingrediente in burguer)
            {
                Debug.Log("Ingrediente: " + ingrediente.Ingrediente);
            }
        }
        Debug.Log("=======================================");
        foreach (List<BurguerJSONCreator.Ingredientes> burguer in match.HamburguesasCorrectas)
        {
            Debug.Log("Hamburguesa Correcta: " + match.HamburguesasCorrectas.IndexOf(burguer));
            foreach (BurguerJSONCreator.Ingredientes ingrediente in burguer)
            {
                Debug.Log("Ingrediente: " + ingrediente.Ingrediente);
            }
        }
        Debug.Log("=======================================");
        foreach (BurguerJSONCreator.HamburguesasEquipos equipo in match.Equipos)
        {
            Debug.Log("Equipo: " + equipo.ID);
            foreach (List<BurguerJSONCreator.IngredientesColocados> burguer in equipo.HamburguesasEntregadas)
            {
                Debug.Log("Hamburguesa Ejemplo: " + equipo.HamburguesasEntregadas.IndexOf(burguer));
                foreach (BurguerJSONCreator.IngredientesColocados ingrediente in burguer)
                {
                    Debug.Log("Ingrediente: " + ingrediente.Ingrediente + " Colocado Por: " + ingrediente.ColocadoPor);
                }
            }
        }
    }
}
