using System;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantDatabaseService : DatabaseConnection
{
    private const string Table = "Games";

    [Serializable] private class PostRequest    { public string username; public string password; public string table; public RestaurantData data; }
    [Serializable] private class RestaurantData { public string DatePlayed; public string TeacherCode; public string ClassPlayed; public string BurguersDelivered; }
    [Serializable] private class GetRequest     { public string username; public string password; public string table; public RestaurantFilter filter; }
    [Serializable] private class RestaurantFilter { public string TeacherCode; public string DatePlayed; public string ClassPlayed; }
    [Serializable] public class GameResponse     { public string result; public List<GameResponseData> data; }
    [Serializable] public class GameResponseData { public string DatePlayed; public string ClassPlayed; public string BurguersDelivered; }

    public void RegisterGame(string teacherCode, string classCode,
        List<List<IngredientsScriptableObject>> recipes, List<List<IngredientsScriptableObject>> orders,
        List<TeamInfo> teams, Dictionary<IngredientsScriptableObject, int> paired, Action<int> callback)
    {
        string matchJson = BurguerJSONCreator.CreateMatchJSON(recipes, orders, teams, paired);
        var req = new PostRequest { username = AppUser, password = AppPass, table = Table,
            data = new RestaurantData { DatePlayed = DateTime.Today.ToString("yyyy-MM-dd"), TeacherCode = teacherCode, ClassPlayed = classCode, BurguersDelivered = matchJson } };
        Post("insert", JsonUtility.ToJson(req), (ok, _) => callback?.Invoke(ok ? 0 : 1));
    }

    public void GetGames(Action<GameResponse> callback, string date = "", string classCode = "")
    {
        string json = BuildGetJson(Table, PlayerData.ClassCode, date, classCode);
        Post("get", json, (ok, body) => { if (!ok) { callback?.Invoke(null); return; } callback?.Invoke(JsonUtility.FromJson<GameResponse>(body)); });
    }

    private string BuildGetJson(string table, string teacherCode, string date, string classCode)
    {
        string filter = $"\"TeacherCode\":\"{teacherCode}\"";
        if (!string.IsNullOrEmpty(date))      filter += $",\"DatePlayed\":\"{date}\"";
        if (!string.IsNullOrEmpty(classCode)) filter += $",\"ClassPlayed\":\"{classCode}\"";
        return $"{{\"username\":\"{AppUser}\",\"password\":\"{AppPass}\",\"table\":\"{table}\",\"filter\":{{{filter}}}}}";
    }
}
