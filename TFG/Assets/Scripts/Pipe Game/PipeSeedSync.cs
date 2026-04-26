using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sincroniza el seed de generacion del escenario entre servidor y clientes.
/// Debe estar en el mismo GameObject que PipeGridFiller.
/// </summary>
public class PipeSeedSync : NetworkBehaviour
{
    private NetworkVariable<int> networkSeed = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private PipeGridFiller filler;

    private void Awake()
    {
        filler = GetComponent<PipeGridFiller>();
        if (filler == null) filler = FindFirstObjectByType<PipeGridFiller>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkSeed.OnValueChanged += OnSeedChanged;

        if (IsServer || IsHost)
        {
            int s = filler.GetSeed();
            networkSeed.Value = s;
            filler.Fill(s); // servidor genera directamente
        }
        else if (networkSeed.Value != 0)
            filler.Fill(networkSeed.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        networkSeed.OnValueChanged -= OnSeedChanged;
    }

    private void OnSeedChanged(int prev, int next)
    {
        if (!IsServer) filler.Fill(next);
    }
}
