using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class ChooseGroup : NetworkBehaviour
{
    PlayerStats player;
    [SerializeField] TeamMenager teamManager;
    [SerializeField] Button readyButton;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
        
    }
    public void Cambio()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        CambioServerRPC(NetworkManager.Singleton.LocalClientId, (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)));
        readyButton.gameObject.SetActive(true);
    }
    [ServerRpc (RequireOwnership = false)]
    private void CambioServerRPC(ulong id, int groupNumber)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        player.idGrupo.Value = groupNumber-1;
        
    }
    public void ReadyPlayer()
    {
        ReadyPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
        
    }
    [ServerRpc (RequireOwnership = false)]
    public void ReadyPlayerServerRPC(ulong id)
    {
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamManager.teams[player.idGrupo.Value];
        teamInfo.integrantes.Add(id);
        teamManager.SetPlayerReady(id);
    }
}
