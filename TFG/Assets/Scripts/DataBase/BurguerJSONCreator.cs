using System.Collections.Generic;
using System.Linq;

public class BurguerJSONCreator
{
    string CreateMatchJSON(List<List<IngredientBehaviour>> hamburguesasEjemplo, List<List<IngredientBehaviour>> hamburguesasCorrectas, List<TeamInfoRestaurante> Equipos)
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
            for (int j = 0; j < Equipos[i].order.Count; j++)
            {
                json += CreateDeliveredBurguerJSON(Equipos[i].order[j], j);
                if (j != Equipos[i].order.Count)
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

    string CreateBurguerJSON(List<IngredientBehaviour> ingredientList, int hamburguesaIDX)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{""Hamburguesa {hamburguesaIDX}"":[";
        foreach (IngredientBehaviour ingredient in ingredientList)
        {
            json += $@" {{""Ingrediente"":""{ingredient.ingredient.IngredientName}""}}";
            if (!ingredient.Equals(ingredientList.Last()))
            {
                json += ",";
            }
            json += "]";
        }
        return json;
    }

    string CreateDeliveredBurguerJSON(List<IngredientBehaviour> ingredientList, int hamburguesaIDX)
    {
        //Construye JSON para la petición REST         
        string json = $@"{{""Hamburguesa {hamburguesaIDX}"":[";
        foreach (IngredientBehaviour ingredient in ingredientList)
        {
            json += $@" {{""Ingrediente"":""{ingredient.ingredient.IngredientName}"",";
            json += $@" ""Colocado por"":""{ingredient.ingredient}""}}";
            if (!ingredient.Equals(ingredientList.Last()))
            {
                json += ",";
            }
            json += "]";
        }
        return json;
    }

}
