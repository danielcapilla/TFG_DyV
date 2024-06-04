using System.Collections.Generic;
using System.Linq;

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
        string json = $@"{{""Hamburguesa {hamburguesaIDX}"":[";
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
        string json = $@"{{""Hamburguesa {hamburguesaIDX}"":[";
        foreach (IngredientBehaviour ingredient in ingredientList)
        {
            json += $@" {{""Ingrediente"":""{ingredient.ingredient.IngredientName}"",";
            json += $@" ""Colocado por"":""{ingredient.playerName}""}}";
            if (!ingredient.Equals(ingredientList.Last()))
            {
                json += ",";
            }
            json += "]";
        }
        return json;
    }

}
