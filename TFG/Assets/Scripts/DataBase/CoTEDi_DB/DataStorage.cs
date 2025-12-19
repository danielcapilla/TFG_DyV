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
using UnityEngine.Rendering;

public class DataStorage : MonoBehaviour
{
    public static DataStorage Instance { get; private set; }

    #region Data Classes
    public class DataClass
    {
        // Base class for data classes
    }

    [Serializable]
    public class UserData : DataClass
    {
        public string UserID;
        public string Name;

    }

    [Serializable]
    public class GameData : DataClass
    {
        public int GameID;
        public string GameStartTime;
        public string GameEndTime;
        public string GameAux1 = "";
        public string GameAux2 = "";
    }

    [Serializable]
    public class HamburguersInfo : DataClass
    {
        public string HamburguersCodes;
        public string CodesMeaning;
        public string RequestedHamburguers;
    }

    [Serializable]
    public class InteractionData : DataClass
    {
        public string Interactions;
        public string HamburguersDelivered;
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

    public static string GetDataJson(DataClass data)
    {
        return JsonUtility.ToJson(data);
    }

    public string GetCombinedDataJson()
    {
        JObject finalJson = new JObject();
        JsonMergeSettings mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union };

        finalJson.Merge(JObject.Parse(GetDataJson(userData)), mergeSettings);
        finalJson.Merge(JObject.Parse(GetDataJson(gameData)), mergeSettings);
        finalJson.Merge(JObject.Parse(GetDataJson(hamburguersInfo)), mergeSettings);
        finalJson.Merge(JObject.Parse(GetDataJson(interactionData)), mergeSettings);


        return finalJson.ToString();
    }

    private byte[] CreateZipFromJsons(Dictionary<string, string> jsonFiles)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var kvp in jsonFiles)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(kvp.Key);
                    using (StreamWriter writer = new StreamWriter(entry.Open()))
                    {
                        writer.Write(kvp.Value);
                    }
                }
            }
            return memoryStream.ToArray();
        }
    }

    public void SaveCombinedJsonToFile()
    {
        try
        {
            string FolderName = $"{Application.persistentDataPath}/DataCollection/{userData.UserID}_Date_" +
                                $"{System.DateTime.Now.Year}_{System.DateTime.Now.Month}_{System.DateTime.Now.Day}_ " +
                                $"{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}/";

            // Zip data
            Dictionary<string, string> jsonFiles = new Dictionary<string, string>();


            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);


            // Fill jsonFiles dictionary
            jsonFiles.Add("CombinedData.json", GetCombinedDataJson());

            byte[] zipBytes = CreateZipFromJsons(jsonFiles);

            //! Deprecated, not recommended to use in itch.io builds, people might freak out when a download starts automatically
            #if UNITY_WEBGL && !UNITY_EDITOR
                // DownloadFile($"{userData.UserID}_Session{sessionData.SessionID}_Game{gameData.GameID}_Time{sessionTime}.zip", zipBytes, zipBytes.Length);
            #else
                File.WriteAllBytes(Path.Combine(FolderName, "Games.zip"), zipBytes);
            #endif

            Debug.Log("JSON data saved to " + Path.Combine(FolderName, $"Games.zip"));


        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save JSON data: " + e.Message);
        }
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
