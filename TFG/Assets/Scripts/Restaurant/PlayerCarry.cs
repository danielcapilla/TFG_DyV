using Unity.Netcode;
using UnityEngine;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField]
    private Transform CarryPosition;
    public bool isCarrying = false;
    public ICarryObject carryingObject { get; private set; }
    [SerializeField]
    private TeamMenager teamManager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        teamManager = GameObject.Find("TeamManager").GetComponent<TeamMenager>();
    }
    public void CarryObject(ICarryObject carryObject)
    {
        Debug.Log("Attemp carry object");
        CarryObject(carryObject.GetNetworkObject());
        CarryObjectClientRPC(carryObject.GetNetworkObject());
    }
    public void CarryObject(NetworkObjectReference carryObjectNetworkObjectReference)
    {
        Debug.Log("Carry Server RPC");
        carryObjectNetworkObjectReference.TryGet(out NetworkObject carryObjectNetworkObject);
        ICarryObject carryObject = carryObjectNetworkObject.GetComponent<ICarryObject>();
        carryObjectNetworkObject.gameObject.transform.localPosition = CarryPosition.localPosition;
        carryingObject = carryObject;
        isCarrying = true;
        //CarryObjectClientRPC(carryObjectNetworkObjectReference);
    }
    [ClientRpc]
    public void CarryObjectClientRPC(NetworkObjectReference carryObjectNetworkObjectReference)
    {
        Debug.Log("Client Server RPC");
        CarryObject(carryObjectNetworkObjectReference);
    }
    public ICarryObject DropObject()
    {
        ICarryObject temp = carryingObject;
        if (carryingObject != null)
        {
            carryingObject = null;
            DropObject(temp.GetNetworkObject());
            //temp.GetGameObject().transform.parent = null;
        }
        //give object to other script
        isCarrying = false;
        return temp;
    }
    private void DropObject(NetworkObjectReference tempNetworkObjectReference)
    {
        tempNetworkObjectReference.TryGet(out NetworkObject carryObjectNetworkObject);
        ICarryObject temp = carryObjectNetworkObject.GetComponent<ICarryObject>();

        temp.GetGameObject().transform.parent = null;
    }
    public override void OnNetworkDespawn()
    {
        if (carryingObject != null)
        {
            foreach (ICarryObject objToDestroy in carryingObject.GetGameObject().transform.GetComponentsInChildren<ICarryObject>())
            {
                objToDestroy.GetNetworkObject().Despawn();
            }
        }
        teamManager.QuitPlayerFromTheTeamServerRPC(OwnerClientId, gameObject.GetComponent<PlayerStats>().idGrupo.Value);
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }


}
