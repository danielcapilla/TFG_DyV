using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class MovementsBehaviour : NetworkBehaviour
{
    [Header("Animación")]
    [SerializeField] private float appearDuration = 0.25f;
    [SerializeField] private float removeDuration = 0.15f;
    [SerializeField] private float idleSwingAngle = 8f;
    [SerializeField] private float idleSwingDuration = 2f; 

    [Header("Referencias")]
    [SerializeField] private GameObject Movement;
    [SerializeField] private GameObject HorizontalLayout;
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private TeamManager teamMenager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        groupBehaviour.OnCommandAdded += HandleCommandAdded;
        if (IsServer)
            groupBehaviour.OnExecutedTurn += HandleExecutedTurn;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        groupBehaviour.OnCommandAdded -= HandleCommandAdded;
        if (IsServer)
            groupBehaviour.OnExecutedTurn -= HandleExecutedTurn;
    }

    private void HandleExecutedTurn(PlayerInputController controller, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        RemoveMovesForClientsClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = teamInfo.integrantes.ToArray()
            }
        });
    }

    [Rpc(SendTo.Server)]
    private void HandleSpawnRPC(int idGroup, CommandType commandType)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        SpawnMoveForClientsClientRPC(commandType, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = teamInfo.integrantes.ToArray()
            }
        });
    }

    private void HandleCommandAdded(ulong id, CommandType commandType)
    {
        int idGrupo = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<PlayerStats>().idGrupo.Value;
        HandleSpawnRPC(idGrupo, commandType);
    }

    [ClientRpc]
    private void RemoveMovesForClientsClientRPC(ClientRpcParams clientRpcParams = default)
    {
        foreach (Transform child in HorizontalLayout.transform)
        {
            DOTween.Kill(child);
            child.DOScale(Vector3.zero, removeDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() => Destroy(child.gameObject));
        }
    }

    [ClientRpc]
    private void SpawnMoveForClientsClientRPC(CommandType commandType, ClientRpcParams clientRpcParams = default)
    {
        GameObject move = Instantiate(Movement, HorizontalLayout.transform);

        int childIndex = commandType switch
        {
            CommandType.MoveUp => 0,
            CommandType.MoveDown => 1,
            CommandType.MoveLeft => 2,
            CommandType.MoveRight => 3,
            CommandType.Wait => 4,
            _ => -1
        };

        move.transform.GetChild(childIndex).gameObject.SetActive(true);

        move.transform.localScale = Vector3.zero;
        move.transform
            .DOScale(Vector3.one, appearDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                StartIdleSwing(move.transform);
            });
    }

    private void StartIdleSwing(Transform move)
    {
        float swingAngle = UnityEngine.Random.Range(idleSwingAngle * 0.9f, idleSwingAngle * 1.1f);
        float duration = UnityEngine.Random.Range(idleSwingDuration * 1.3f, idleSwingDuration * 1.7f);
        float randomDelay = UnityEngine.Random.Range(0f, 0.6f);

        float startAngle = UnityEngine.Random.Range(-swingAngle, swingAngle);
        move.localRotation = Quaternion.Euler(0f, 0f, startAngle);

        move.DORotate(new Vector3(0f, 0f, -swingAngle), duration / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .From(new Vector3(0f, 0f, swingAngle)) 
            .SetDelay(randomDelay);
    }
}
