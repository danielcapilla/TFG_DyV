using Unity.Netcode;
using UnityEngine;

public class PipeGameConfigSync : GameConfigSyncBase
{
    [SerializeField] private PipeGameConfigSO config;
    protected override GameConfigBaseSO BaseConfig => config;

    private NetworkVariable<int>   netColumns    = new(4,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int>   netRows       = new(4,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int>   netReserve    = new(6,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netGapRatio   = new(0.35f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netLockRatio  = new(0.4f,  NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int>   netExtraTiles = new(3,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected override void OnServerSpawn()
    {
        netColumns.Value    = config.columns;
        netRows.Value       = config.rows;
        netReserve.Value    = config.reserveSlots;
        netGapRatio.Value   = config.gapRatio;
        netLockRatio.Value  = config.lockRatio;
        netExtraTiles.Value = config.extraTiles;
    }

    protected override void OnClientSpawn()
    {
        netColumns.OnValueChanged    += (_, v) => config.columns      = v;
        netRows.OnValueChanged       += (_, v) => config.rows         = v;
        netReserve.OnValueChanged    += (_, v) => config.reserveSlots = v;
        netGapRatio.OnValueChanged   += (_, v) => config.gapRatio     = v;
        netLockRatio.OnValueChanged  += (_, v) => config.lockRatio    = v;
        netExtraTiles.OnValueChanged += (_, v) => config.extraTiles   = v;
        config.columns      = netColumns.Value;
        config.rows         = netRows.Value;
        config.reserveSlots = netReserve.Value;
        config.gapRatio     = netGapRatio.Value;
        config.lockRatio    = netLockRatio.Value;
        config.extraTiles   = netExtraTiles.Value;
    }
}
