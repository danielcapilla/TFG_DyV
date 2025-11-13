using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class InvertControlsPowerUp : PowerUp
{
    [Header("Configuración")]
    [SerializeField] private float inversionDuration = 10f;
    // Parecido al mutex, evitar multiples regodidas
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);

    // Cosa que no sabia, si se spawnea un objeto en el servidor, 
    // las fisica se ejecutan en el cliente que las ha spawneado (en este caso el server)
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
    }

    [Rpc(SendTo.Server)]
    private void RequestActivatePowerUpRPC(ulong playerClientId)
    {
        if (isPickedUp.Value) return;
        isPickedUp.Value = true;

        //if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(playerClientId, out var client))
        //    return;

        //int pickerGroupId = client.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        int pickerGroupId = NetworkManager.Singleton.ConnectedClients[playerClientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamMenager teamManager = FindFirstObjectByType<TeamMenager>();
        if (teamManager != null)
        {
            // Afectar a los demas grupos no a mi
            List<ulong> affectedClients = new List<ulong>();

            for (int i = 0; i < teamManager.teams.Count; i++)
            {
                if (i == pickerGroupId) continue;

                TeamInfoChicken teamInfo = (TeamInfoChicken)teamManager.teams[i];
                // Agregar varios elementos
                affectedClients.AddRange(teamInfo.integrantes);
            }

            // Notificamos a los agraciados
            if (affectedClients.Count > 0)
            {
                InvertControlsClientRpc(inversionDuration, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = affectedClients.ToArray()
                    }
                });
            }

            // Notificamos al que lo recogio
            TeamInfoChicken pickerTeamInfo = (TeamInfoChicken)teamManager.teams[pickerGroupId];
            if (pickerTeamInfo.integrantes.Count > 0)
            {
                NotifyPickerGroupClientRpc(affectedClients.Count, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = pickerTeamInfo.integrantes.ToArray()
                    }
                });
            }
        }

        // Ocultar y destruir el power-up
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
    private void InvertControlsClientRpc(float duration, ClientRpcParams clientRpcParams = default)
    {
        PowerUpEvents.InvokeInvertControls(duration);
    }

    [ClientRpc]
    private void NotifyPickerGroupClientRpc(int affectedPlayersCount, ClientRpcParams clientRpcParams = default)
    {
        PlayPopPickUpSound();
    }

    public override void RemoveEffect(GameObject target)
    {
    }
}
