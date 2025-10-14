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
    public class GroupsEndSnapshot
    {   
        public int GroupId;
        public string[] PlayersId;
        public Vector2Int GridPos; 
    }

    public static string CreateGridMatchJSON(List<GridLevelSnapshot> grid, List<GroupsEndSnapshot> groups)
    {
        string json = "";
        json += "{";

        // Grid 
        json += "'Grid':[";
        for (int i = 0; i < grid.Count; i++)
        {
            json += CreateLevelJSON(grid[i], i);
            if (i != grid.Count - 1) json += ",";
        }
        json += "],";

        // Groups
        json += "'Groups':[";
        for (int i = 0; i < groups.Count; i++)
        {
            json += CreateGroupJSON(groups[i]);
            if (i != groups.Count - 1) json += ",";
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

    static string CreateGroupJSON(GroupsEndSnapshot g)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        sb.AppendFormat("'Group':{0},", g.GroupId);

        sb.Append("'PlayersId':[");
        if (g.PlayersId != null && g.PlayersId.Length > 0)
        {
            for (int i = 0; i < g.PlayersId.Length; i++)
            {
                string pid = (g.PlayersId[i] ?? string.Empty).Replace("'", "");
                sb.AppendFormat("'{0}'", pid);
                if (i != g.PlayersId.Length - 1) sb.Append(",");
            }
        }
        sb.Append("],");

        sb.AppendFormat("'FinalGridPos':{{'x':{0},'y':{1}}}", g.GridPos.x, g.GridPos.y);
        sb.Append("}");
        return sb.ToString();
    }

    public static GridMatch CreateMatchObject(string data)
    {
        string aux = data.Replace("'", "\"");
        GridMatchData md = JsonUtility.FromJson<GridMatchData>(aux);

        GridMatch match = new GridMatch();
        if (md.Grid != null)
            match.Grids.AddRange(md.Grid);
        if (md.Groups != null)
            match.Groups.AddRange(md.Groups);
        return match;
    }

    [System.Serializable]
    class GridMatchData
    {
        public GridData[] Grid;
        public GroupData[] Groups;
    }

    [System.Serializable]
    public class GridMatch
    {
        public List<GridData> Grids = new();
        public List<GroupData> Groups = new();
    }

    [System.Serializable]
    public class GridData
    {
        public string ID;
        public int Width;
        public int Height;
        public int[] Grid;
        public Vector2Int Start;
        public Vector2Int Goal;
    }

    [System.Serializable]
    public class GroupData
    {
        public int Group;
        public string[] PlayerId;
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