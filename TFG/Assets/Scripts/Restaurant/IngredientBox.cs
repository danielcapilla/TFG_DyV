using Unity.Netcode;
using UnityEngine;

public class IngredientBox : InteractableObject
{
    public IngredientsScriptableObject ingredient;
    [SerializeField] GameObject Lid;


    private void Awake()
    {
        //GameObject instance = Instantiate(ingredient.Model, transform);
        //instance.transform.localPosition = new Vector3(0, 0.5f, 0);
        GameObject lid = Instantiate(Lid, transform);
        lid.transform.localPosition = new Vector3(0, 0.8f, 0);
        GameObject plane = new GameObject("Plane");
        plane.transform.parent = transform;
        plane.transform.Rotate(new Vector3(90, 0, 0));
        SpriteRenderer renderer = plane.AddComponent<SpriteRenderer>();
        renderer.sprite = ingredient.Sprite;
        plane.transform.localPosition = new Vector3(0, 1.01f, -ingredient.Sprite.bounds.size.y / 4);
        plane.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        SpawnMultiplayerServerRPC(player.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnMultiplayerServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {

        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        if (!playerCarry.isCarrying)
        {
            GameObject instance = Instantiate(ingredient.Model);
            ICarryObject carryObject = instance.GetComponent<IngredientBehaviour>();
            //Para que lo vean los clientes
            NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn(true);


            //Poner de padre al player que lo ha invocado (solo funciona si se pone en una serverRPC)
            carryObject.GetGameObject().transform.SetParent(playerCarry.transform);
            carryObject.GetGameObject().GetComponent<IngredientBehaviour>().ingredient = ingredient;
            playerCarry.CarryObject(carryObject);
        }
    }
    //Missing box display its content and highlight when player can interact
}
