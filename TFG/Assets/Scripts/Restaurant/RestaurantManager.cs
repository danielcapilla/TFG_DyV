using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RestaurantManager : NetworkBehaviour
{
    [SerializeField]
    private RecipeRandomizer recipe;
    [SerializeField]
    private TeamMenager teamMenager;

    private void Start()
    {
        NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
    }

    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        if(!IsServer || !IsHost) return;
     
        Debug.Log(BurguerJSONCreator.CreateMatchJSON(recipe.recipes,recipe.currentOrders, teamMenager.teams));
    }
}
