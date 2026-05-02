using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sincroniza los parametros comunes de GameConfigBaseSO del servidor a los clientes.
/// Sobrescribir OnServerSpawn y OnClientSpawn para sincronizar parametros especificos.
/// </summary>
public abstract class GameConfigSyncBase : NetworkBehaviour
{
    private NetworkVariable<float> netMatchDuration = new(180f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int>   netRounds        = new(1,    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int>   netMaxPlayers    = new(4,    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected abstract GameConfigBaseSO BaseConfig { get; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            netMatchDuration.Value = BaseConfig.matchDuration;
            netRounds.Value        = BaseConfig.rounds;
            netMaxPlayers.Value    = BaseConfig.maxPlayers;
            OnServerSpawn();
        }
        else
        {
            netMatchDuration.OnValueChanged += (_, v) => BaseConfig.matchDuration = v;
            netRounds.OnValueChanged        += (_, v) => BaseConfig.rounds        = v;
            netMaxPlayers.OnValueChanged    += (_, v) => BaseConfig.maxPlayers    = v;
            BaseConfig.matchDuration = netMatchDuration.Value;
            BaseConfig.rounds        = netRounds.Value;
            BaseConfig.maxPlayers    = netMaxPlayers.Value;
            OnClientSpawn();
        }
    }

    protected virtual void OnServerSpawn() { }
    protected virtual void OnClientSpawn() { }
}
