using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerRestaurant : GameManagerBase
{
    [Header("Restaurante - Referencias")]
    [SerializeField] private AudioSource restaurantMusic;
    [SerializeField] private RecipeRandomizer recipeRandomizer;
    [SerializeField] private TeamManager teamMenager;
    [SerializeField] private RestaurantDatabaseService restaurantService;

    private string studentClassCode = "A";

    private void Start()
    {
        if (IsClient && !IsHost)
            ClassCodeServerRPC(PlayerData.ClassCode);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ClassCodeServerRPC(FixedString64Bytes classCode)
    {
        studentClassCode = classCode.ToString();
    }

    protected override void OnGameStarted()
    {
        restaurantMusic?.Play();
    }

    protected override void OnTimerFinished()
    {
        restaurantService.RegisterGame(
            PlayerData.ClassCode,
            studentClassCode,
            recipeRandomizer.recipes,
            recipeRandomizer.currentOrders,
            teamMenager.teams,
            recipeRandomizer.pairedIngredients,
            AllowChangeScene);
    }

    private void AllowChangeScene(int result)
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
