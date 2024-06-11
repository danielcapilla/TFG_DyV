using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class RestaurantManager : NetworkBehaviour
{
    [SerializeField]
    private RecipeRandomizer recipe;
    [SerializeField]
    private TeamMenager teamMenager;
    [SerializeField]
    private DataBaseCommander dataBaseCommander;
    private string studentClassCode;

    private AsyncOperation Async;
    private void Start()
    {
        if (IsClient && !IsHost)
        {
            ClassCodeServerRPC(PlayerData.ClassCode);
        }
        if (!IsServer || !IsHost) return;
        NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClassCodeServerRPC(FixedString64Bytes classCode)
    {
        studentClassCode = classCode.ToString();
    }
    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        asyncOperation.allowSceneActivation = false;
        Async = asyncOperation;
        dataBaseCommander.RegisterGame(PlayerData.ClassCode, studentClassCode, recipe.recipes, recipe.currentOrders, teamMenager.teams, recipe.pairedIngredients, AllowChangeScene);

    }

    void AllowChangeScene(int result)
    {
        Async.allowSceneActivation = true;
    }
}
