using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeDatabaseService : DatabaseConnection
{
    private const string Table = "PipeGames";

    [Serializable] private class PostRequest { public string username; public string password; public string table; public PipeGameData data; }
    [Serializable] private class PipeGameData { public string DatePlayed; public string TeacherCode; public string ClassPlayed; public string MatchData; }

    [Serializable] private class GetRequest { public string username; public string password; public string table; public PipeFilter filter; }
    [Serializable] private class PipeFilter { public string TeacherCode; public string DatePlayed; public string ClassPlayed; }
    [Serializable] public class GameResponsePipe     { public string result; public System.Collections.Generic.List<GameResponseDataPipe> data; }
    [Serializable] public class GameResponseDataPipe { public string DatePlayed; public string ClassPlayed; public string MatchData; }

    public void GetGames(Action<GameResponsePipe> callback, string date = "", string classCode = "")
    {
        string json = BuildGetJson(Table, PlayerData.ClassCode, date, classCode);
        Post("get", json, (ok, body) =>
        {
            if (!ok) { callback?.Invoke(null); return; }
            callback?.Invoke(JsonUtility.FromJson<GameResponsePipe>(body));
        });
    }

    private string BuildGetJson(string table, string teacherCode, string date, string classCode)
    {
        string filter = $"\"TeacherCode\":\"{teacherCode}\"";
        if (!string.IsNullOrEmpty(date))      filter += $",\"DatePlayed\":\"{date}\"";
        if (!string.IsNullOrEmpty(classCode)) filter += $",\"ClassPlayed\":\"{classCode}\"";
        return $"{{\"username\":\"{AppUser}\",\"password\":\"{AppPass}\",\"table\":\"{table}\",\"filter\":{{{filter}}}}}";
    }

    public void RegisterGame(PipeMatchData matchData, Action<int> callback)
    {
        string json = JsonUtility.ToJson(matchData);
        var req = new PostRequest { username = AppUser, password = AppPass, table = Table,
            data = new PipeGameData
            {
                DatePlayed  = matchData.date,
                TeacherCode = matchData.teacherCode,
                ClassPlayed = matchData.classCode,
                MatchData   = json
            }};
        Post("insert", JsonUtility.ToJson(req), (ok, _) => callback?.Invoke(ok ? 0 : 1));
    }
}
