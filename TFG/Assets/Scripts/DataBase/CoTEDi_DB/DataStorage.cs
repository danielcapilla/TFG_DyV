using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Localization.Settings;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Linq;

public class DataStorage : MonoBehaviour
{
    public static DataStorage Instance { get; private set; }

    #region Data Classes

    [Serializable]
    public class UserData
    {
        public string UserID;
        public string Name;

    }

    [Serializable]
    public class GameData
    {
        public int GameID;
        public DateTime StartTime;
        public DateTime EndTime;
        public string Aux1 = "";
        public string Aux2 = "";
    }

    [Serializable]
    public class HamburguersInfo
    {
        public string HamburguersCodes;
        public string CodesMeaning;
        public string RequestedHamburguers;
    }

    [Serializable]
    public class InteractionData
    {
        public string Interactions;
        public string DeliveredHamburguers;
        public string Movement;

    }

    #endregion


    // Containers
    public UserData userData;
    public GameData gameData;
    public HamburguersInfo hamburguersInfo;
    public InteractionData interactionData;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartClasses();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("TRIED TO CREATE A SINGLETON TWO TIMES");
        }
    }
    
    private void StartClasses()
    {
        userData = new UserData();
        gameData = new GameData();
        hamburguersInfo = new HamburguersInfo();
        interactionData = new InteractionData();

    }

}

[Serializable]
public class ListWrapper<T>
{
    public List<T> items;

    public ListWrapper()
    {
        items = new List<T>();
    }

    public void Add(T item)
    {
        items.Add(item);
    }

    public override string ToString()
    {
        string result = string.Join(", ", items);
        result = "[" + result + "]";
        return result;
    }

}

[Serializable]
public class DictionaryWrapper<TKey, TValue>
{
    public Dictionary<TKey, TValue> dict;

    public DictionaryWrapper()
    {
        dict = new Dictionary<TKey, TValue>();
    }

    public void Add(TKey key, TValue value)
    {
        dict.Add(key, value);
    }

    public override string ToString()
    {
        List<string> dictPairs = dict.Select(kv => $"\"{kv.Key.ToString()}\": \"{kv.Value.ToString()}\"").ToList();
        
        string result = string.Join(", ", dictPairs);
        result = "{" + result + "}";
        return result;
    }
}
