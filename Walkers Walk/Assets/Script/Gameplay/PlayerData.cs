using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public struct PlayerSerializableData
{
    public int currency;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    private PlayerSerializableData data;

    public Action OnDeathAction;

    private string filePath => Path.Combine(Application.dataPath, "GameData", "settings.gamedata");

    public void IncreaseCurrency(int amount)
    {
        data.currency += amount;
        Debug.Log("Currency increased" + data.currency);
    }

    public void DecreaseCurrency(int amount)
    {
        if (data.currency - amount < 0)
        {
            return;
        }

        data.currency -= amount;

        Debug.Log("Currency decreased" + data.currency);
    }

    public bool CanAfford(int amount)
    {
        return data.currency >= amount;
    }

    public void OnDeath()
    {
        OnDeathAction?.Invoke();
    }

    public void Save()
    {
        // Crear directorio si no existe
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Guardar los datos
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fileStream, data);
        }

        Debug.Log($"Game saved at: {filePath}");
    }

    public void Load()
    {
        if (File.Exists(filePath))
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                data = (PlayerSerializableData)bf.Deserialize(fileStream);
            }

            Debug.Log("Game loaded");
        }
        else
        {
            Debug.LogWarning($"Save file not found at: {filePath}");
        }
    }

    public string GetCurrency()
    {
        return data.currency.ToString();
    }
}
