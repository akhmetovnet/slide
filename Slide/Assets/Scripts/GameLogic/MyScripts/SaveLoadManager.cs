using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
[System.Serializable]
public class SaveLoadManager
{
    public static List<PlayerData> savedGames = new List<PlayerData>();
    
    public static void Save()
    {
        // savedGames.Add(PlayerData.instance);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
        bf.Serialize(file,SaveLoadManager.savedGames);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath, FileMode.Open);
            SaveLoadManager.savedGames = (List<PlayerData>) bf.Deserialize(file);
            file.Close();
        }
    }
}
