using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class ChooseGroup : NetworkBehaviour
{
    PlayerStats player;
    [SerializeField] TeamMenager teamMenager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
    }
    public void Cambio()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        CambioServerRPC(NetworkManager.Singleton.LocalClientId, (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)));
    }
    [ServerRpc (RequireOwnership = false)]
    private void CambioServerRPC(ulong id, int groupNumber)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        player.idGrupo.Value = groupNumber-1;
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamMenager.teams[player.idGrupo.Value];
        teamInfo.integrantes.Add(id);
    }
}
