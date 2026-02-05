using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InteractionsCollector : MonoBehaviour
{

    #region Auxiliary class
    /// <summary>
    ///  Aux class to store interaction data
    /// </summary>
    private class Interaction
    {
        private int time { get; set; }
        private int interactionType { get; set; }
        private int necessaryInteraction { get; set; }
        private int objectInteradtedWith { get; set; }
        private int deliver { get; set; }

        public Interaction(int time, int interactionType, int necessaryInteraction, int objectInteradtedWith, int deliver)
        {
            this.time = time;
            this.interactionType = interactionType;
            this.necessaryInteraction = necessaryInteraction;
            this.objectInteradtedWith = objectInteradtedWith;
            this.deliver = deliver;
        }

        public override string ToString()
        {
            return $"({time}, {interactionType}, {necessaryInteraction}, {objectInteradtedWith}, {deliver})";
        }
    }

    #endregion

    [SerializeField] DataStorage dataStorage;
    List<Interaction> interactions = new List<Interaction>();
    
    public void AddInteraction(int time, int interactionType, int necessaryInteraction, int objectInteractedWith, int deliver)
    {
        Interaction interaction = new Interaction(time, interactionType, necessaryInteraction, objectInteractedWith, deliver);
        interactions.Add(interaction);
    }

    public void SaveInteractions()
    {
        string interactionsString = "[";
        for (int i = 0; i < interactions.Count; i++)
        {
            interactionsString += interactions[i].ToString();
            if (i < interactions.Count - 1)
            {
                interactionsString += ", ";
            }
        }
        interactionsString += "]";
        dataStorage.interactionData.Interactions = interactionsString;
    }
}



