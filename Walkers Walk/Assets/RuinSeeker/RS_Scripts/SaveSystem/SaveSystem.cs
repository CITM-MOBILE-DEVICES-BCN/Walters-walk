using UnityEngine;
using System.IO;
using System.Collections.Generic;

public interface ISaveSystem
{
    void Save(SaveData saveData);
    SaveData Load();
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int stars;
    public int highestGems;
}

[System.Serializable]
public class SaveData
{
    public Vector2 lastCheckpointPosition;
    public int totalScore;
    public int totalGems;
    public List<LevelData> levelProgress = new List<LevelData>();
}

public class SaveSystem : ISaveSystem
{
    private string filePath;

    public SaveSystem()
    {
        filePath = Application.persistentDataPath + "/setting.json";
    }

    public void Save(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to: " + filePath);
    }

    public SaveData Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData.levelProgress == null)
                saveData.levelProgress = new List<LevelData>();

            return saveData; ;
        }
        else
        {
            Debug.LogWarning("Save file not found. Returning default SaveData.");
            return new SaveData();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.LogWarning("Save file not found.");
        }
    }
}