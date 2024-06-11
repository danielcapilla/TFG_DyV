using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestaurantManager : NetworkBehaviour
{
    [SerializeField]
    private RecipeRandomizer recipe;
    [SerializeField]
    private TeamMenager teamMenager;
    [SerializeField]
    private DataBaseCommander dataBaseCommander;
    private string studentClassCode = "B";

    private void Start()
    {
        if (IsClient && !IsHost)
        {
            ClassCodeServerRPC(PlayerData.ClassCode);
        }
        if (!IsServer || !IsHost) return;
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClassCodeServerRPC(FixedString64Bytes classCode)
    {
        studentClassCode = classCode.ToString();
    }
    public void SaveMatchToDB()
    {
        dataBaseCommander.RegisterGame(PlayerData.ClassCode, studentClassCode, recipe.recipes, recipe.currentOrders, teamMenager.teams, recipe.pairedIngredients, AllowChangeScene);
    }

    void AllowChangeScene(int result)
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
