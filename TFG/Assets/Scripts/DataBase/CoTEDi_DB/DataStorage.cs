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
    public abstract class DataClass
    {
        // Base class for data classes
        public abstract void OnAwakeData();
    }

    [Serializable]
    public class UserData : DataClass
    {
        public string UserID;
        public string Name;
        public string Age;
        public string UserAux1;
        public string UserAux2;

        public override void OnAwakeData()
        {
            UserID = "";
            Name = "";
            Age = "";
            UserAux1 = "";
            UserAux2 = "";
        }

        public void SetUserData(string name, string age)
        {
            Name = name.Replace(" ", "_").ToLower();
            UserID = GenerateUserID(Name);
        }

        private string GenerateUserID(string name)
        {
            // Function by https://stackoverflow.com/questions/63615950/generate-unique-id-from-string-in-c-sharp
            string hash;
            using (var hashAlgorithm = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)  
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                hash = sBuilder.ToString();
            }
            return hash;
        }
    }

    [Serializable]
    public class GameData : DataClass
    {
        public int GameID;
        public string GameStartTime;
        public string GameEndTime;
        public string GameAux1 = "";
        public string GameAux2 = "";

        public override void OnAwakeData()
        {
            GameID = 0;
            GameStartTime = "";
            GameEndTime = "";
        }

        public void SetGameTime(bool isStartTime)
        {
            string currentTime = System.DateTime.Now.ToString("M/d/yyyy/HH:mm:ss");
            if (isStartTime)
                GameStartTime = currentTime;
            else
                GameEndTime = currentTime;
        }
    }

    [Serializable]
    public class HamburguersInfo : DataClass
    {
        public string HamburguersCodes;
        public string CodesMeaning;
        public string RequestedHamburguers;
        public string HamburgerInfoAux1;
        public string HamburgerInfoAux2;

        public override void OnAwakeData()
        {
            HamburguersCodes = "";
            CodesMeaning = "";
            RequestedHamburguers = "";
            HamburgerInfoAux1 = "";
            HamburgerInfoAux2 = "";
        }
    }

    [Serializable]
    public class InteractionData : DataClass
    {
        public string Interactions;
        public string HamburguersDelivered;
        public string Movement;
        public string InteractionDataAux1;
        public string InteractionDataAux2;

        public override void OnAwakeData()
        {
            Interactions = "";
            HamburguersDelivered = "";
            Movement = "";
            InteractionDataAux1 = "";
            InteractionDataAux2 = "";
        }
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
            InitData();
            Debug.Log(userData.UserID);
            Debug.Log(userData.Name);
            Debug.Log(userData.Age);
            Debug.Log(gameData.GameAux1);

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

        userData.OnAwakeData();
        gameData.OnAwakeData();
        hamburguersInfo.OnAwakeData();
        interactionData.OnAwakeData();
    }

    private void InitData()
    {
        Debug.Log("Edad: " + PlayerData.Age);
        // TODO: Fix this ToString
        userData.SetUserData(PlayerData.Name, PlayerData.Age.ToString());
        gameData.SetGameTime(true);
    }

    private void EndData()
    {
        gameData.SetGameTime(false);
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
