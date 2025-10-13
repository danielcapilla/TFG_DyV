using System.Collections.Generic;
using TMPro;
using UI.Dates;
using UnityEngine;

public class FiltersBehaviour : MonoBehaviour
{
    public enum GameType { Restaurant, Chicken }

    [SerializeField]
    private DataBaseCommander db;
    [SerializeField]
    private DatePicker datePicker;
    [SerializeField]
    private TextMeshProUGUI codeText;

    // Modo seleccionado 
    public GameType selectedGameType;

    public string date;
    public string code;

    // Restaurant
    public DataBaseCommander.GameResponse gameResponse;
    public BurguerJSONCreator.Match match;

    // Chicken
    public DataBaseCommander.GameResponseChicken chickenResponse;
    public GridJSONCreator.GridMatch chickenMatch;


    public void SetGameType(GameType type)
    {
        selectedGameType = type;
    }

    public void Filter()
    {
        // codigo de clase con seguro
        string raw = codeText != null ? codeText.text : string.Empty;
        code = string.IsNullOrEmpty(raw) || raw.Length < 1 ? "" : raw.Substring(0, raw.Length - 1);

        // fecha seleccionada 
        try
        {
            date = datePicker.SelectedDate.Date.ToString("yyyy-MM-dd");
        }
        catch (System.Exception)
        {
            date = "";
        }

        // Llamada segun modo
        if (selectedGameType == GameType.Restaurant)
        {
            db.GetGame(GetGamesRestaurant, date, code);
        }
        else
        {
            db.GetChickenGame(GetGamesChicken, date, code);
        }
    }

    private void OnDisable()
    {
        if (codeText != null) codeText.text = "";
    }

    // Callback Restaurant
    public void GetGamesRestaurant(DataBaseCommander.GameResponse data)
    {
        gameResponse = data;
        if (gameResponse != null && gameResponse.data != null && gameResponse.data.Count > 0)
        {
            match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[0].BurguersDelivered);
        }
    }

    // Callback Chicken
    public void GetGamesChicken(DataBaseCommander.GameResponseChicken data)
    {
        chickenResponse = data;
        Debug.Log(chickenResponse);
        if (chickenResponse != null && chickenResponse.data != null && chickenResponse.data.Count > 0)
        {
            chickenMatch = GridJSONCreator.CreateMatchObject(chickenResponse.data[0].Grid);
        }
    }

    // Set juego activo 
    public void SetGame(int game)
    {
        if (selectedGameType == GameType.Restaurant)
        {
            if (gameResponse != null && gameResponse.data != null && game >= 0 && game < gameResponse.data.Count)
                match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[game].BurguersDelivered);
        }
        else
        {
            if (chickenResponse != null && chickenResponse.data != null && game >= 0 && game < chickenResponse.data.Count)
                chickenMatch = GridJSONCreator.CreateMatchObject(chickenResponse.data[game].Grid);
        }
    }

    // Cantidad de partidas cargadas
    public int GetGamesCount()
    {
        return selectedGameType == GameType.Restaurant
            ? (gameResponse?.data?.Count ?? 0)
            : (chickenResponse?.data?.Count ?? 0);
    }

    public void ClearData()
    {
        if (selectedGameType == GameType.Restaurant)
            gameResponse?.data?.Clear();
        else
            chickenResponse?.data?.Clear();
    }

    // Debug solo para Restaurant
    public void ShowMatch()
    {
        if (match == null) return;

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
            foreach (BurguerJSONCreator.HamburguesaEntregada burguer in equipo.HamburguesasEntregadas)
            {
                Debug.Log("Hamburguesa Ejemplo: " + equipo.HamburguesasEntregadas.IndexOf(burguer));
                foreach (BurguerJSONCreator.IngredientesColocados ingrediente in burguer.Hamburguesa)
                {
                    Debug.Log("Ingrediente: " + ingrediente.Ingrediente + " Colocado Por: " + ingrediente.ColocadoPor);
                }
            }
        }
    }
}