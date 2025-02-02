using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveSystem
{
    public static bool SaveMap(MapData map)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/maps";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path += "/" + map.mapName;

        Debug.Log("Saved at " + path);
        FileStream file = new FileStream(path, FileMode.Create);
        formatter.Serialize(file, map);
        file.Close();

        return true;
    }

    public static MapData Load(string levelName)
    {
        string path = Application.persistentDataPath + "/maps/" + levelName;

        if (!File.Exists(path))
        {
            Debug.Log("File not found at " + path);
            return null;
        }

        BinaryFormatter formatter = new BinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try
        {
            Debug.Log("Loaded at " + path);
            MapData save = formatter.Deserialize(file) as MapData;
            file.Close();
            return save;
        }
        catch
        {
            Debug.Log("Failed load at " + path);
            file.Close();
            return null;
        }
    }

    public static bool MapExists(string levelName)
    {
        return File.Exists(Application.persistentDataPath + "/maps/" + levelName);
    }
}
