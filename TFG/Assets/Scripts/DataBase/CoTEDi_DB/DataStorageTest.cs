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
            ListWrapper<string> testList = new ListWrapper<string>();
            testList.items = new System.Collections.Generic.List<string> { "0", "1", "103", "2", "JALI" };

            ListWrapper<ListWrapper<string>> hamburguersList = new ListWrapper<ListWrapper<string>>();
            hamburguersList.items.Add(testList);

            Debug.Log(hamburguersList.ToString());
            string tes = hamburguersList.ToString();
            string json = JsonUtility.ToJson(DataStorage.Instance.hamburguersInfo);

            Debug.Log(json);

            DictionaryWrapper<string, string> dictTest = new DictionaryWrapper<string, string>();
            
            dictTest.Add("Key1", "Value1");
            dictTest.Add("Key2", "Value2");
            
            Debug.Log(dictTest.ToString());
        }
    
        // List<List<string>> 
        
    
    }
}