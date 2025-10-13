using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class GridJSONCreator
{
    [System.Serializable]
    public class GridLevelSnapshot
    {
        public int Width;
        public int Height;
        public int[] Grid;         
        public Vector2Int Start;
        public Vector2Int Goal;
    }

    [System.Serializable]
    public class PlayerEndSnapshot
    {
        public string PlayerId;    
        public int GroupId;        
        public Vector3 WorldPos;   
        public Vector2Int GridPos; 
    }

    public static string CreateGridMatchJSON(List<GridLevelSnapshot> levels, List<PlayerEndSnapshot> players)
    {
        var inv = CultureInfo.InvariantCulture;

        string json = "";
        json += "{";

        // Levels
        json += "'Levels':[";
        for (int i = 0; i < levels.Count; i++)
        {
            json += CreateLevelJSON(levels[i], i);
            if (i != levels.Count - 1) json += ",";
        }
        json += "],";

        // Players
        json += "'Players':[";
        for (int i = 0; i < players.Count; i++)
        {
            json += CreatePlayerJSON(players[i], inv);
            if (i != players.Count - 1) json += ",";
        }
        json += "]";

        json += "}";
        return json;
    }

    static string CreateLevelJSON(GridLevelSnapshot lvl, int index)
    {
        string json = $@"{{'ID':'Level {index}','Width':{lvl.Width},'Height':{lvl.Height},";
        json += $@"'Start':{{'x':{lvl.Start.x},'y':{lvl.Start.y}}},";
        json += $@"'Goal':{{'x':{lvl.Goal.x},'y':{lvl.Goal.y}}},";
        json += "'Grid':[";
        for (int j = 0; j < lvl.Grid.Length; j++)
        {
            json += lvl.Grid[j].ToString();
            if (j != lvl.Grid.Length - 1) json += ",";
        }
        json += "]}";
        return json;
    }

    static string CreatePlayerJSON(PlayerEndSnapshot p, CultureInfo inv)
    {
        string pid = (p.PlayerId ?? string.Empty).Replace("'", ""); 
        string json = $@"{{'PlayerId':'{pid}','Group':{p.GroupId},";
        json += $@"'FinalWorldPos':{{'x':{p.WorldPos.x.ToString(inv)},'y':{p.WorldPos.y.ToString(inv)},'z':{p.WorldPos.z.ToString(inv)}}},";
        json += $@"'FinalGridPos':{{'x':{p.GridPos.x},'y':{p.GridPos.y}}}}}";
        return json;
    }

    public static GridMatch CreateMatchObject(string data)
    {
        string aux = data.Replace("'", "\"");
        GridMatchData md = JsonUtility.FromJson<GridMatchData>(aux);

        GridMatch match = new GridMatch();
        if (md.Levels != null)
            match.Levels.AddRange(md.Levels);
        if (md.Players != null)
            match.Players.AddRange(md.Players);
        return match;
    }

    [System.Serializable]
    class GridMatchData
    {
        public LevelData[] Levels;
        public PlayerData[] Players;
    }

    [System.Serializable]
    public class GridMatch
    {
        public List<LevelData> Levels = new();
        public List<PlayerData> Players = new();
    }

    [System.Serializable]
    public class LevelData
    {
        public string ID;
        public int Width;
        public int Height;
        public int[] Grid;
        public Vector2Int Start;
        public Vector2Int Goal;
    }

    [System.Serializable]
    public class PlayerData
    {
        public string PlayerId;
        public int Group;
        public Vector3 FinalWorldPos;
        public Vector2Int FinalGridPos;
    }
    public static int[] FlattenGrid(int[,] grid, int width, int height)
    {
        int[] flat = new int[width * height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                flat[y * width + x] = grid[x, y];
        return flat;
    }
}