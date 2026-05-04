using System;
using System.Collections.Generic;
using UnityEngine;

public class ChickenDatabaseService : DatabaseConnection
{
    private const string Table = "ChickenGames";

    [Serializable] private class PostRequest  { public string username; public string password; public string table; public ChickenData data; }
    [Serializable] private class ChickenData  { public string DatePlayed; public string TeacherCode; public string ClassPlayed; public string Grid; }
    [Serializable] private class GetRequest   { public string username; public string password; public string table; public ChickenFilter filter; }
    [Serializable] private class ChickenFilter { public string TeacherCode; public string DatePlayed; public string ClassPlayed; }
    [Serializable] public class GameResponseChicken     { public string result; public List<GameResponseDataChicken> data; }
    [Serializable] public class GameResponseDataChicken { public string DatePlayed; public string ClassPlayed; public string Grid; }

    public void RegisterGame(string teacherCode, string classCode,
        List<GridJSONCreator.GridLevelSnapshot> levels, List<GridJSONCreator.GroupsEndSnapshot> groups, Action<int> callback)
    {
        string matchJson = GridJSONCreator.CreateGridMatchJSON(levels, groups);
        var req = new PostRequest { username = AppUser, password = AppPass, table = Table,
            data = new ChickenData { DatePlayed = DateTime.Today.ToString("yyyy-MM-dd"), TeacherCode = teacherCode, ClassPlayed = classCode, Grid = matchJson } };
        Post("insert", JsonUtility.ToJson(req), (ok, _) => callback?.Invoke(ok ? 0 : 1));
    }

    public void RegisterCurrentGame(string teacherCode, string classCode, Action<int> callback)
    {
        var gen = GridLevelGenerator.Instance;
        if (gen == null) { callback?.Invoke(1); return; }
        gen.GetSnapshot(out int w, out int h, out int[] flat, out Vector2Int start, out Vector2Int goal);
        var levels = new List<GridJSONCreator.GridLevelSnapshot> { new GridJSONCreator.GridLevelSnapshot { Width = w, Height = h, Grid = flat, Start = start, Goal = goal } };
        var groups = new List<GridJSONCreator.GroupsEndSnapshot>();
        var tm = TeamManager.Instance;
        if (tm?.teams == null) { callback?.Invoke(1); return; }
        foreach (var team in tm.teams)
        {
            if (team == null || team.ID == -1 || team.integrantes == null || team.integrantes.Count == 0) continue;
            var tc = (TeamInfoChicken)team;
            var ids = new List<string>();
            foreach (var cid in team.integrantes) ids.Add(cid.ToString());
            groups.Add(new GridJSONCreator.GroupsEndSnapshot { GroupId = team.ID, PlayersId = ids.ToArray(), GridPos = tc.playerPrefab.CurrentGridPos });
        }
        RegisterGame(teacherCode, classCode, levels, groups, callback);
    }

    public void GetGames(Action<GameResponseChicken> callback, string date = "", string classCode = "")
    {
        var req = new GetRequest { username = AppUser, password = AppPass, table = Table,
            filter = new ChickenFilter { TeacherCode = PlayerData.ClassCode, DatePlayed = date, ClassPlayed = classCode } };
        Post("get", JsonUtility.ToJson(req), (ok, body) => { if (!ok) { callback?.Invoke(null); return; } callback?.Invoke(JsonUtility.FromJson<GameResponseChicken>(body)); });
    }
}
