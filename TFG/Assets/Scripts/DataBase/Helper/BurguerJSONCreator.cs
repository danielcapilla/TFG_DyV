using System.Collections.Generic;
using UnityEngine;

public class BurguerJSONCreator
{
    public static string CreateMatchJSON(List<List<IngredientsScriptableObject>> hamburguesasEjemplo, List<List<IngredientsScriptableObject>> hamburguesasCorrectas, List<TeamInfo> Equipos, Dictionary<IngredientsScriptableObject, int> PairedIngredients)
    {
        //Construye JSON para la petición REST

        //string json = $@"{{'HamburguesasEjemplo':[]}}";
        string json = "";

        json += $@"{{ 'HamburguesasEjemplo':[";
        for (int i = 0; i < hamburguesasEjemplo.Count; i++)
        {
            json += CreateBurguerJSON(hamburguesasEjemplo[i], i, PairedIngredients);
            if (i != hamburguesasEjemplo.Count - 1)
            {
                json += ",";
            }
        }
        json += "],";


        json += $@"'HamburguesasCorrectas':[";
        for (int i = 0; i < hamburguesasCorrectas.Count; i++)
        {
            json += CreateBurguerJSON(hamburguesasCorrectas[i], i, PairedIngredients);
            if (i != hamburguesasCorrectas.Count - 1)
            {
                json += ",";
            }
        }
        json += "],";



        json += $@"'Equipos':[";
        for (int i = 0; i < Equipos.Count; i++)
        {
            json += $@"{{'Equipo': '{i}',";
            json += $@"'Hamburguesas': [";
            TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)Equipos[i];
            for (int j = 0; j < teamInfo.Burguers.Count; j++)
            {
                json += CreateDeliveredBurguerJSON(teamInfo.Burguers[j].burguer, j, teamInfo.Burguers[j].idOrder);
                if (j != teamInfo.Burguers.Count - 1)
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

        json += $@"]}}";

        return json;
    }

    static string CreateBurguerJSON(List<IngredientsScriptableObject> ingredientList, int hamburguesaIDX, Dictionary<IngredientsScriptableObject, int> PairedIngredients)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{'ID': 'Hamburguesa {hamburguesaIDX}' ,'Ingredientes':[";
        int i = 0;
        foreach (IngredientsScriptableObject ingredient in ingredientList)
        {
            json += $@" {{'Ingrediente':{ingredient.ID},";
            json += $@" 'Codigo':{PairedIngredients[ingredient]}}}";
            if (i != ingredientList.Count - 1)
            {
                json += ",";
            }
            i++;
        }
        json += "]}";
        return json;
    }

    static string CreateDeliveredBurguerJSON(List<IngredientBehaviour> ingredientList, int hamburguesaIDX, int IDPedido)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{'ID': 'Hamburguesa {hamburguesaIDX}', 'IDPedido' : {IDPedido} ,'Ingredientes':[";
        int i = 0;
        foreach (IngredientBehaviour ingredient in ingredientList)
        {
            json += $@" {{'Ingrediente':{ingredient.ingredient.ID},";
            json += $@" 'ColocadoPor':'{ingredient.playerName}'}}";
            if (i != ingredientList.Count - 1)
            {
                json += ",";
            }
            i++;
        }
        json += "]}";
        return json;
    }


    public static Match CreateMatchObject(string data)
    {
        Match match = new Match();
        string aux = data.Replace("'", "\"");
        Debug.Log(aux);
        MatchData MatchData = JsonUtility.FromJson<MatchData>(aux);
        //HamburguesasEjemplo
        for (int i = 0; i < MatchData.HamburguesasEjemplo.Length; i++)
        {
            //BurguerData burguerData = CreateBurguerObject(MatchData.HamburguesasEjemplo[i]);
            List<Ingredientes> burguer = new List<Ingredientes>();
            burguer.AddRange(MatchData.HamburguesasEjemplo[i].Ingredientes);
            match.HamburguesasEjemplo.Add(burguer);
        }

        //HamburguesasCorrectas
        for (int i = 0; i < MatchData.HamburguesasCorrectas.Length; i++)
        {
            //BurguerData burguerData = CreateBurguerObject(MatchData.HamburguesasCorrectas[i]);
            List<Ingredientes> burguer = new List<Ingredientes>();
            burguer.AddRange(MatchData.HamburguesasCorrectas[i].Ingredientes);
            match.HamburguesasCorrectas.Add(burguer);
        }

        for (int i = 0; i < MatchData.Equipos.Length; i++)
        {
            //EquiposJSONData equipo = JsonUtility.FromJson<EquiposJSONData>(MatchData.Equipos[i]);
            HamburguesasEquipos hamburguesasEquipos = new HamburguesasEquipos();
            hamburguesasEquipos.ID = i.ToString();
            for (int j = 0; j < MatchData.Equipos[i].Hamburguesas.Length; j++)
            {
                //BurguerDeliveredData burguerDelivered = CreateDeliveredBurguerObject(equipo.HamburguesasEntregadas[j]);
                List<IngredientesColocados> burguer = new List<IngredientesColocados>();
                burguer.AddRange(MatchData.Equipos[i].Hamburguesas[j].Ingredientes);
                HamburguesaEntregada deliveredBurguer = new();
                deliveredBurguer.ID = j.ToString();
                deliveredBurguer.IDOrder = MatchData.Equipos[i].Hamburguesas[j].IDPedido;
                deliveredBurguer.Hamburguesa = burguer;
                hamburguesasEquipos.HamburguesasEntregadas.Add(deliveredBurguer);
            }
            match.Equipos.Add(hamburguesasEquipos);
        }
        return match;
    }

    static BurguerData CreateBurguerObject(string data)
    {
        BurguerData burguerData = JsonUtility.FromJson<BurguerData>(data);
        Debug.Log(data);
        Debug.Log(burguerData);
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
        public BurguerData[] HamburguesasEjemplo;
        public BurguerData[] HamburguesasCorrectas;
        public EquiposJSONData[] Equipos;
    }

    [System.Serializable]
    public class Match
    {
        public List<List<Ingredientes>> HamburguesasEjemplo = new();
        public List<List<Ingredientes>> HamburguesasCorrectas = new();
        public List<HamburguesasEquipos> Equipos = new();
    }

    [System.Serializable]
    class EquiposJSONData
    {
        public string Equipo;
        public BurguerDeliveredData[] Hamburguesas;
    }

    [System.Serializable]
    public class HamburguesasEquipos
    {
        public string ID;
        public List<HamburguesaEntregada> HamburguesasEntregadas = new();
    }

    [System.Serializable]
    public class HamburguesaEntregada
    {
        public string ID;
        public int IDOrder;
        public List<IngredientesColocados> Hamburguesa;
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
        public int IDPedido;
        public IngredientesColocados[] Ingredientes;
    }

    [System.Serializable]
    public class Ingredientes
    {
        public int Ingrediente;
        public int Codigo;
    }

    [System.Serializable]
    public class IngredientesColocados
    {
        public int Ingrediente;
        public string ColocadoPor;
    }
}
