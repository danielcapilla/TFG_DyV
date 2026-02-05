using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

public class HamburgersInfo_Collector : MonoBehaviour
{
    [SerializeField] private DataStorage dataStorage;
    
    public List<RequestedHamburgerInfo> requestedHamburgers = new List<RequestedHamburgerInfo>();


    public void SetHamburguersCodes(List<List<IngredientsScriptableObject>> hamburgerRecepies)
    {
        string hamburgersCodes = "[";

        for (int i = 0; i < hamburgerRecepies.Count; i++)
        {
            List<IngredientsScriptableObject> currentHamburger = hamburgerRecepies[i];
            string currentHamburgerCode = "(";

            for (int j = 0; j < currentHamburger.Count; j++)
            {
                IngredientsScriptableObject currentIngredient = currentHamburger[j];
                currentHamburgerCode += $"{{{currentIngredient.ID.ToString()}, {j.ToString()}}}";
                if (j < currentHamburger.Count - 1)
                {
                    currentHamburgerCode += ", ";
                }
            }

            currentHamburgerCode += ")";
            hamburgersCodes += currentHamburgerCode;

            if (i < hamburgerRecepies.Count - 1)
            {
                hamburgersCodes += ", ";
            }

        }

        hamburgersCodes += "]";


        dataStorage.hamburguersInfo.HamburguersCodes = hamburgersCodes;
    }

    public void SetCodesMeaning(Dictionary<IngredientsScriptableObject, int> ingredientsCodesMeaning)
    {
        string codesMeaning = "[";
        int count = 0;
        foreach (var kvp in ingredientsCodesMeaning)
        {
            codesMeaning += $"({kvp.Key.ID.ToString()}, {kvp.Value})";
            count++;
            if (count < ingredientsCodesMeaning.Count - 1)
            {
                codesMeaning += ", ";
            }
        }
        codesMeaning += "]";
        dataStorage.hamburguersInfo.CodesMeaning = codesMeaning;
        Debug.Log(codesMeaning);
    }

    public void SetAllRequestsIngredients(List<List<IngredientsScriptableObject>> ingredients)
    {
        for(int i = 0; i < ingredients.Count; i++)
        {
            requestedHamburgers.Add(new RequestedHamburgerInfo());
            requestedHamburgers[i].NumRequest = i;
            requestedHamburgers[i].Ingredients = ingredients[i];
            Debug.Log($"Requested hamburguer {i}: " + requestedHamburgers[i].ToString());
        }
    }

    public void RequestedHamburguersToString()
    {
        string requestedString = "[";
        for (int i = 0; i < requestedHamburgers.Count; i++)
        {
            string foo = $"{{{requestedHamburgers[i].ToString()}}}";
            if (i < requestedHamburgers.Count - 1)
            {
                foo += ", ";
            }
            requestedString += foo;
        }
        requestedString += "]";
        dataStorage.hamburguersInfo.RequestedHamburguers = requestedString;
        Debug.Log(requestedString);
    }

}

public class RequestedHamburgerInfo
{
    public int NumRequest;
    public int MomentRequested;
    public int TimeRequested;
    public int NumFails;
    public List<IngredientsScriptableObject> Ingredients;

    public RequestedHamburgerInfo(int numRequest, int momentRequested, int timeRequested, int numFails, List<IngredientsScriptableObject> ingredients)
    {
        NumRequest = numRequest;
        MomentRequested = momentRequested;
        TimeRequested = timeRequested;
        NumFails = numFails;
        Ingredients = ingredients;
    }

    public RequestedHamburgerInfo()
    {
        NumRequest = 0;
        MomentRequested = 0;
        TimeRequested = 0;
        NumFails = 0;
        Ingredients = new List<IngredientsScriptableObject>();
    }

    public override string ToString()
    {
        string ingredientsStr = "";
        for(int i = 0; i < Ingredients.Count; i++)
        {
            ingredientsStr += Ingredients[i].ID;
            if (i < Ingredients.Count - 1)
            {
                ingredientsStr += ", ";
            }
        }

        return $"{NumRequest}, {MomentRequested}, {TimeRequested}, {NumFails}, {{{string.Join(", ", ingredientsStr)}}}";
    }
}