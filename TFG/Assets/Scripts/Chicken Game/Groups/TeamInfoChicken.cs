using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfoChicken", menuName = "Teams/TeamInfoChicken")]
public class TeamInfoChicken : TeamInfo
{
    //public int idOrder;
    //public List<DeliveredBurguerInfo> Burguers = new();
    //public Action<int> OnIdOrderChange;
    public bool spawnedPlayer = false;
    public int turn = 0;
    public Queue<ICommand> commandQueue = new Queue<ICommand>();
    public PlayerInputController playerPrefab;
    public override TeamInfo Clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfoChicken>();
    }
}
