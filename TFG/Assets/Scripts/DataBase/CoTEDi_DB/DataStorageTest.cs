using System.Collections;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
   
namespace Assets.Scripts.DataBase.CoTEDi_DB
{
    public class DataStorageTest : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            TestDB();
        }

        private void TestDB()
        {
            DataStorage.Instance.userData.UserID = "User123";
            DataStorage.Instance.userData.Name = "Test User";

            DataStorage.Instance.gameData.GameID = 42;
            DataStorage.Instance.gameData.GameStartTime = System.DateTime.Now.ToString();
            DataStorage.Instance.gameData.GameEndTime = System.DateTime.Now.AddHours(1).ToString();
            DataStorage.Instance.gameData.GameAux1 = "Auxiliary Data 1";

            DataStorage.Instance.hamburguersInfo.HamburguersCodes = "Code1,Code2,Code3";
            DataStorage.Instance.hamburguersInfo.CodesMeaning = "Meaning1,Meaning2,Meaning3";
            DataStorage.Instance.hamburguersInfo.RequestedHamburguers = "Burger1,Burger2";

            DataStorage.Instance.interactionData.Interactions = "Interaction1,Interaction2";
            DataStorage.Instance.interactionData.HamburguersDelivered = "Burger1";
            DataStorage.Instance.interactionData.Movement = "Up,Down,Left,Right";

            Debug.Log("DataStorage Test Completed Successfully.");
            DataStorage.Instance.SaveCombinedJsonToFile();

        }
    
    }
}