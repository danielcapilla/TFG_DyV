using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BurguerJSONCreator
{
    public static string CreateMatchJSON(List<List<IngredientsScriptableObject>> hamburguesasEjemplo, List<List<IngredientsScriptableObject>> hamburguesasCorrectas, List<TeamInfo> Equipos)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{ ""HamburguesasEjemplo"":[";
        for (int i = 0; i < hamburguesasEjemplo.Count; i++)
        {
            json += CreateBurguerJSON(hamburguesasEjemplo[i], i);
            if (i != hamburguesasEjemplo.Count - 1)
            {
                json += ",";
            }
        }
        json += "],";


        json += $@"""HamburguesasCorrectas"":[";
        for (int i = 0; i < hamburguesasCorrectas.Count; i++)
        {
            json += CreateBurguerJSON(hamburguesasCorrectas[i], i);
            if (i != hamburguesasCorrectas.Count - 1)
            {
                json += ",";
            }
        }
        json += "],";

        json += $@"""Equipos"":[";
        for (int i = 0; i < Equipos.Count; i++)
        {
            json += $@"""Equipo"": ""Equipo {i}"",";
            json += $@"""Hamburguesas"": [";
            TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)Equipos[i];
            for (int j = 0; j < teamInfo.order.Count; j++)
            {
                json += CreateDeliveredBurguerJSON(teamInfo.order[j], j);
                if (j != teamInfo.order.Count)
                {
                    json += ",";
                }
            }
            json += $@"]}}";
            if (i != Equipos.Count - 1)
            {
                json += ",";
            }
        }
        json += $@"]}}}}";
        return json;
    }

    static string CreateBurguerJSON(List<IngredientsScriptableObject> ingredientList, int hamburguesaIDX)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{""ID"": ""Hamburguesa {hamburguesaIDX}"" ,""Ingredientes"":[";
        foreach (IngredientsScriptableObject ingredient in ingredientList)
        {
            json += $@" {{""Ingrediente"":""{ingredient.IngredientName}""}}";
            if (!ingredient.Equals(ingredientList.Last()))
            {
                json += ",";
            }
            json += "]";
        }
        return json;
    }

    static string CreateDeliveredBurguerJSON(List<IngredientBehaviour> ingredientList, int hamburguesaIDX)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{""ID"": ""Hamburguesa {hamburguesaIDX}"" ,""Ingredientes"":[";
        foreach (IngredientBehaviour ingredient in ingredientList)
        {
            json += $@" {{""Ingrediente"":""{ingredient.ingredient.IngredientName}"",";
            json += $@" ""ColocadoPor"":""{ingredient.playerName}""}}";
            if (!ingredient.Equals(ingredientList.Last()))
            {
                json += ",";
            }
            json += "]";
        }
        return json;
    }


    static Match CreateMatchObject(string data)
    {
        Match match = new Match();
        MatchData MatchData = JsonUtility.FromJson<MatchData>(data);
        //HamburguesasEjemplo
        for (int i = 0; i < MatchData.HamburguesasEjemplo.Length; i++)
        {
            BurguerData burguerData = CreateBurguerObject(MatchData.HamburguesasEjemplo[i]);
            List<Ingredientes> burguer = new List<Ingredientes>();
            burguer.AddRange(burguerData.Ingredientes);
            match.HamburguesasEjemplo.Add(burguer);
        }

        //HamburguesasCorrectas
        for (int i = 0; i < MatchData.HamburguesasCorrectas.Length; i++)
        {
            BurguerData burguerData = CreateBurguerObject(MatchData.HamburguesasCorrectas[i]);
            List<Ingredientes> burguer = new List<Ingredientes>();
            burguer.AddRange(burguerData.Ingredientes);
            match.HamburguesasEjemplo.Add(burguer);
        }

        for (int i = 0; i < MatchData.Equipos.Length; i++)
        {
            EquiposJSONData equipo = JsonUtility.FromJson<EquiposJSONData>(MatchData.Equipos[i]);
            HamburguesasEquipos hamburguesasEquipos = new HamburguesasEquipos();
            hamburguesasEquipos.ID = i;
            for (int j = 0; j < equipo.HamburguesasEntregadas.Length; j++)
            {
                BurguerDeliveredData burguerDelivered = CreateDeliveredBurguerObject(equipo.HamburguesasEntregadas[j]);
                List<IngredientesColocados> burguer = new List<IngredientesColocados>();
                burguer.AddRange(burguerDelivered.Ingredientes);
                hamburguesasEquipos.HamburguesasEntregadas.Add(burguer);
            }
            match.Equipos.Add(hamburguesasEquipos);
        }
        return match;
    }

    static BurguerData CreateBurguerObject(string data)
    {
        BurguerData burguerData = JsonUtility.FromJson<BurguerData>(data);
        return burguerData;
    }

    static BurguerDeliveredData CreateDeliveredBurguerObject(string data)
    {
        BurguerDeliveredData burguerDeliveredData = JsonUtility.FromJson<BurguerDeliveredData>(data);
        return burguerDeliveredData;
    }

    [System.Serializable]
    class MatchData
    {
        public string[] HamburguesasEjemplo;
        public string[] HamburguesasCorrectas;
        public string[] Equipos;
    }

    [System.Serializable]
    class Match
    {
        public List<List<Ingredientes>> HamburguesasEjemplo = new();
        public List<List<Ingredientes>> HamburguesasCorrectas = new();
        public List<HamburguesasEquipos> Equipos = new();
    }

    [System.Serializable]
    class EquiposJSONData
    {
        public string Equipo;
        public string[] HamburguesasEntregadas;
    }

    [System.Serializable]
    class HamburguesasEquipos
    {
        public int ID;
        public List<List<IngredientesColocados>> HamburguesasEntregadas;
    }

    [System.Serializable]
    class BurguerData
    {
        public string ID;
        public Ingredientes[] Ingredientes;
    }

    [System.Serializable]
    class BurguerDeliveredData
    {
        public string ID;
        public IngredientesColocados[] Ingredientes;
    }

    [System.Serializable]
    class Ingredientes
    {
        public string Ingrediente;
    }

    [System.Serializable]
    class IngredientesColocados
    {
        public string Ingrediente;
        public string ColocadoPor;
    }
}
