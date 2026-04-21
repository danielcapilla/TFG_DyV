using Unity.Netcode;
using UnityEngine;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private Transform carryPosition;

    public bool isCarrying => carryingObject != null;
    public ICarryObject carryingObject { get; private set; }


    public bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    // ── SetParentSafe ─────────────────────────────────────────────────────────
    // Desactiva AutoObjectParentSync en toda la jerarquia antes de SetParent.
    // Funciona tanto para objetos spawneados (online) como no spawneados (offline).
    public static void SetParentSafe(GameObject go, Transform newParent)
    {
        NetworkObject[] netObjs = go.GetComponentsInChildren<NetworkObject>(true);
        foreach (var no in netObjs)
            no.AutoObjectParentSync = false;
        go.transform.SetParent(newParent);
        foreach (var no in netObjs)
            no.AutoObjectParentSync = true;
    }

    // ── Coger ─────────────────────────────────────────────────────────────────

    public bool TryPickUp(ICarryObject carryObject)
    {
        if (isCarrying || carryObject == null || !carryObject.CanBePickedUp) return false;
        if (!IsOffline && carryObject.GetNetworkObject() == null) return false;

        if (IsOffline)
            PickUpLocally(carryObject);
        else
            PickUpServerRPC(carryObject.GetNetworkObject());

        return true;
    }

    public void CarryObject(ICarryObject carryObject) => TryPickUp(carryObject);

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickUpServerRPC(NetworkObjectReference objRef)
    {
        if (!objRef.TryGet(out NetworkObject netObj)) return;
        PickUpClientRPC(objRef);
    }

    [ClientRpc]
    private void PickUpClientRPC(NetworkObjectReference objRef)
    {
        if (!objRef.TryGet(out NetworkObject netObj)) return;
        ICarryObject obj = netObj.GetComponent<ICarryObject>();
        if (obj == null) return;
        carryingObject = obj;
        // SetParentSafe desactiva AutoObjectParentSync para que Netcode no interfiera
        // con el reparentado local a un transform no-NetworkObject
        SetParentSafe(netObj.gameObject, carryPosition);
        netObj.transform.localPosition = Vector3.zero;
        netObj.transform.localRotation = Quaternion.identity;
        obj.OnPickedUp(this);
    }

    public void PickUpLocally(ICarryObject carryObject)
    {
        carryingObject = carryObject;
        SetParentSafe(carryObject.GetGameObject(), carryPosition);
        carryObject.GetGameObject().transform.localPosition = Vector3.zero;
        carryObject.GetGameObject().transform.localRotation = Quaternion.identity;
        carryObject.OnPickedUp(this);
    }

    // ── Soltar ────────────────────────────────────────────────────────────────

    public ICarryObject DropObject()
    {
        if (!isCarrying) return null;
        ICarryObject dropped = carryingObject;
        carryingObject = null;
        SetParentSafe(dropped.GetGameObject(), null);

        if (!IsOffline)
            DropServerRPC(dropped.GetNetworkObject());

        dropped.OnRejected(null);
        return dropped;
    }

    // El servidor sincroniza el estado de parentesco en Netcode (null = root)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DropServerRPC(NetworkObjectReference objRef)
    {
        // Solo actualiza el estado interno de Netcode sin mover el transform
        // (ya lo movio el cliente via SetParentSafe)
        if (!objRef.TryGet(out NetworkObject netObj)) return;
        netObj.TrySetParent((NetworkObject)null);
    }

    // ── Entregar a receptor ───────────────────────────────────────────────────

    public bool TryDeliver(ICarryReceiver receiver)
    {
        if (!isCarrying || receiver == null) return false;
        if (!receiver.CanReceive(carryingObject, this))
        {
            carryingObject.OnRejected(receiver);
            return false;
        }
        return DeliverLocally(receiver);
    }

    public bool DeliverLocally(ICarryReceiver receiver)
    {
        ICarryObject toDeliver = carryingObject;
        bool accepted = receiver.Receive(toDeliver, this);
        if (accepted)
        {
            carryingObject = null;
            toDeliver.OnDelivered(receiver);
        }
        else
        {
            toDeliver.OnRejected(receiver);
        }
        return accepted;
    }

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    public override void OnNetworkDespawn()
    {
        if (carryingObject != null)
        {
            NetworkObject no = carryingObject.GetNetworkObject();
            if (no != null && no.IsSpawned)
            {
                foreach (ICarryObject obj in carryingObject.GetGameObject()
                             .transform.GetComponentsInChildren<ICarryObject>())
                {
                    NetworkObject objNo = obj.GetNetworkObject();
                    if (objNo != null && objNo.IsSpawned) objNo.Despawn();
                }
            }
        }

    }

    public void SetCarryingObject(ICarryObject obj) { carryingObject = obj; }
    public NetworkObject GetNetworkObject() => NetworkObject;
    public Transform GetCarryPosition() => carryPosition;
}
