using System.Collections.Generic;
using TMPro;
using UI.Dates;
using UnityEngine;

public class FiltersBehaviour : MonoBehaviour
{
    public enum GameType { Restaurant, Chicken, Pipe }

    [SerializeField] private RestaurantDatabaseService restaurantService;
    [SerializeField] private ChickenDatabaseService chickenService;
    [SerializeField] private PipeDatabaseService pipeService;
    [SerializeField] private DatePicker datePicker;
    [SerializeField] private TextMeshProUGUI codeText;

    public GameType selectedGameType;
    public string date;
    public string code;

    // Restaurant
    public RestaurantDatabaseService.GameResponse gameResponse;
    public BurguerJSONCreator.Match match;

    // Chicken
    public ChickenDatabaseService.GameResponseChicken chickenResponse;
    public GridJSONCreator.GridMatch chickenMatch;

    // Pipe
    public System.Collections.Generic.List<PipeMatchData> pipeMatches = new System.Collections.Generic.List<PipeMatchData>();
    public PipeMatchData pipeMatch => pipeMatches.Count > 0 ? pipeMatches[currentPipeIndex] : null;
    private int currentPipeIndex = 0;

    public void SetGameType(GameType type)
    {
        selectedGameType = type;
    }

    public void Filter()
    {
        string raw = codeText != null ? codeText.text : string.Empty;
        code = string.IsNullOrEmpty(raw) || raw.Length < 1 ? "" : raw.Substring(0, raw.Length - 1);

        try { date = datePicker.SelectedDate.Date.ToString("yyyy-MM-dd"); }
        catch (System.Exception) { date = ""; }

        if (selectedGameType == GameType.Restaurant)
            restaurantService.GetGames(GetGamesRestaurant, date, code);
        else if (selectedGameType == GameType.Chicken)
            chickenService.GetGames(GetGamesChicken, date, code);
        else if (selectedGameType == GameType.Pipe)
        {
            if (pipeService != null) pipeService.GetGames(GetGamesPipe, date, code);
            else Debug.LogWarning("[Filters] pipeService no asignado");
        }
    }

    private void OnDisable()
    {
        if (codeText != null) codeText.text = "";
    }

    public void GetGamesRestaurant(RestaurantDatabaseService.GameResponse data)
    {
        gameResponse = data;
        if (gameResponse != null && gameResponse.data != null && gameResponse.data.Count > 0)
            match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[0].BurguersDelivered);
    }

    public void GetGamesPipe(PipeDatabaseService.GameResponsePipe data)
    {
        pipeMatches.Clear();
        currentPipeIndex = 0;
        if (data == null || data.data == null || data.data.Count == 0) return;
        foreach (var entry in data.data)
        {
            var match = JsonUtility.FromJson<PipeMatchData>(entry.MatchData);
            if (match != null) pipeMatches.Add(match);
        }
    }
    public void GetGamesChicken(ChickenDatabaseService.GameResponseChicken data)
    {
        chickenResponse = data;
        if (chickenResponse != null && chickenResponse.data != null && chickenResponse.data.Count > 0)
            chickenMatch = GridJSONCreator.CreateMatchObject(chickenResponse.data[0].Grid);
    }

    public void SetGame(int game)
    {
        if (selectedGameType == GameType.Restaurant)
        {
            if (gameResponse != null && gameResponse.data != null && game >= 0 && game < gameResponse.data.Count)
                match = BurguerJSONCreator.CreateMatchObject(gameResponse.data[game].BurguersDelivered);
        }
        else if (selectedGameType == GameType.Chicken)
        {
            if (chickenResponse != null && chickenResponse.data != null && game >= 0 && game < chickenResponse.data.Count)
                chickenMatch = GridJSONCreator.CreateMatchObject(chickenResponse.data[game].Grid);
        }
        else if (selectedGameType == GameType.Pipe)
        {
            currentPipeIndex = Mathf.Clamp(game, 0, pipeMatches.Count - 1);
            Debug.Log($"[Filters] SetGame Pipe: game={game} idx={currentPipeIndex} total={pipeMatches.Count}");
        }
    }

    public int GetGamesCount()
    {
        if (selectedGameType == GameType.Restaurant) return gameResponse?.data?.Count ?? 0;
        if (selectedGameType == GameType.Chicken)    return chickenResponse?.data?.Count ?? 0;
        if (selectedGameType == GameType.Pipe)       return pipeMatches?.Count ?? 0;
        return 0;
    }

    public void ClearData()
    {
        if (selectedGameType == GameType.Restaurant) gameResponse?.data?.Clear();
        else if (selectedGameType == GameType.Chicken) chickenResponse?.data?.Clear();
        else if (selectedGameType == GameType.Pipe) { pipeMatches.Clear(); currentPipeIndex = 0; }
    }

    public void ShowMatch()
    {
        if (match == null) return;
        foreach (List<BurguerJSONCreator.Ingredientes> burguer in match.HamburguesasEjemplo)
        {
            UnityEngine.Debug.Log("Hamburguesa Ejemplo: " + match.HamburguesasEjemplo.IndexOf(burguer));
            foreach (BurguerJSONCreator.Ingredientes ingrediente in burguer)
                UnityEngine.Debug.Log("Ingrediente: " + ingrediente.Ingrediente);
        }
        UnityEngine.Debug.Log("=======================================");
        foreach (List<BurguerJSONCreator.Ingredientes> burguer in match.HamburguesasCorrectas)
        {
            UnityEngine.Debug.Log("Hamburguesa Correcta: " + match.HamburguesasCorrectas.IndexOf(burguer));
            foreach (BurguerJSONCreator.Ingredientes ingrediente in burguer)
                UnityEngine.Debug.Log("Ingrediente: " + ingrediente.Ingrediente);
        }
        UnityEngine.Debug.Log("=======================================");
        foreach (BurguerJSONCreator.HamburguesasEquipos equipo in match.Equipos)
        {
            UnityEngine.Debug.Log("Equipo: " + equipo.ID);
            foreach (BurguerJSONCreator.HamburguesaEntregada burguer in equipo.HamburguesasEntregadas)
            {
                UnityEngine.Debug.Log("Hamburguesa Ejemplo: " + equipo.HamburguesasEntregadas.IndexOf(burguer));
                foreach (BurguerJSONCreator.IngredientesColocados ingrediente in burguer.Hamburguesa)
                    UnityEngine.Debug.Log("Ingrediente: " + ingrediente.Ingrediente + " Colocado Por: " + ingrediente.ColocadoPor);
            }
        }
    }
}
