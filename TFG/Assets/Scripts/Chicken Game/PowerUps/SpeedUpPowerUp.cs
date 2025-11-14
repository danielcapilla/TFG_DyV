using Unity.Netcode;
using UnityEngine;

public class SpeedUpPowerUp : PowerUp
{
    [SerializeField] private float speedMultiplier = 2f;

    protected void OnTriggerEnter(Collider go)
    {

        // Si ya fue recogido, no hacer nada (por si entra otro cliente 0.001 segundos despues)
        if (isPickedUp.Value) return;

        PlayerInputController player = go.GetComponent<PlayerInputController>();
        if (player != null)
        {
            ApplyEffect(player.gameObject);
        }
    }

    public override void ApplyEffect(GameObject target)
    {
        if (isPickedUp.Value) return;

        PlayerInputController player = target.GetComponent<PlayerInputController>();
        NetworkObject networkObject = player.GetComponentInParent<NetworkObject>();
        RequestActivatePowerUpRPC(networkObject.OwnerClientId);
        // Al hacerse las fisicas en el servidor, el efecto se aplica directamente desde el servidor
        // por lo cual es el servidor quien llama a StartSpeedUp
        // el cambio se realiza integro en el playerInputController
        player.StartSpeedUp(speedMultiplier, duration);
    }

    [Rpc(SendTo.Server)]
    private void RequestActivatePowerUpRPC(ulong playerClientId)
    {
        if (isPickedUp.Value) return;
        isPickedUp.Value = true;

        int pickerGroupId = NetworkManager.Singleton.ConnectedClients[playerClientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamMenager teamManager = FindFirstObjectByType<TeamMenager>();;
        if (teamManager != null)
        {          
            // Notificamos al que lo recogio
            TeamInfoChicken pickerTeamInfo = (TeamInfoChicken)teamManager.teams[pickerGroupId];
            if (pickerTeamInfo.integrantes.Count > 0)
            {
                NotifyPickerGroupClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = pickerTeamInfo.integrantes.ToArray()
                    }
                });
            }
        }

        HideVisualClientRpc();
        Invoke(nameof(DestroyPowerUp), 0.5f);
    }

    private void DestroyPowerUp()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void HideVisualClientRpc()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    [ClientRpc]
    private void NotifyPickerGroupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PowerUpEvents.InvokePlayerSpeedUp(duration);
        PlayPickUpSound();
    }

    public override void RemoveEffect(GameObject target)
    {
      
    }
}
