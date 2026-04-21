using Unity.Netcode;
using UnityEngine;

public class IngredientBox : InteractableObject
{
    public IngredientsScriptableObject ingredient;
    [SerializeField] GameObject Lid;

    private void Awake()
    {
        GameObject lid = Instantiate(Lid, transform);
        lid.transform.localPosition = new Vector3(0, 0.8f, 0);
        GameObject plane = new GameObject("Plane");
        plane.transform.parent = transform;
        plane.transform.Rotate(new Vector3(90, 0, 0));
        SpriteRenderer sr = plane.AddComponent<SpriteRenderer>();
        sr.sprite = ingredient.Sprite;
        plane.transform.localPosition = new Vector3(0, 1.01f, -ingredient.Sprite.bounds.size.y / 4);
        plane.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    protected override void InteractOnline(PlayerCarry player) => SpawnServerRPC(player.GetNetworkObject());

    protected override void InteractOffline(PlayerCarry player)
    {
        if (player.isCarrying) return;
        GameObject instance = Instantiate(ingredient.Model);
        IngredientBehaviour carryObject = instance.GetComponent<IngredientBehaviour>();
        carryObject.ingredient = ingredient;
        player.TryPickUp(carryObject);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        if (playerCarry.isCarrying) return;

        GameObject instance = Instantiate(ingredient.Model);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        instance.GetComponent<IngredientBehaviour>().ingredient = ingredient;
        // CarryObject -> TryPickUp -> PickUpServerRPC -> PickUpClientRPC en todos
        // PickUpClientRPC hace SetParentSafe(carryPosition) en cada cliente localmente
        playerCarry.CarryObject(instance.GetComponent<IngredientBehaviour>());
    }
}
