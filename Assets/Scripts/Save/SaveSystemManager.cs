using System;
using System.IO;
using UnityEngine;

public class SaveSystemManager : Singleton<SaveSystemManager>
{
    [SerializeField] private SaveSystemData saveSystemData;

    private static event Action onSaveOrLoadAllGame;

    private string path => Application.persistentDataPath + "/save.json";

    public SaveSystemData SaveSystemData { get => saveSystemData; }

    public static Action OnSavOrLoadAllGame { get => onSaveOrLoadAllGame; set => onSaveOrLoadAllGame = value; }


    void Awake()
    {
        CreateSingleton(true);
        //DeleteAllData();
    }


    public static void SaveGame(SaveData data)
    {
        if (!instance.saveSystemData.UseSaveSystem) return;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(instance.path, json);
    }

    public static SaveData LoadGame()
    {
        if (!instance.saveSystemData.UseSaveSystem) return new SaveData();

        if (File.Exists(instance.path))
        {
            string json = File.ReadAllText(instance.path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        return new SaveData();
    }

    public static bool SaveExists()
    {
        if (!instance.saveSystemData.UseSaveSystem) return false;

        return File.Exists(instance.path);
    }

    public static void DeleteAllData()
    {
        //if (!instance.saveSystemData.UseSaveSystem) return;

        if (File.Exists(instance.path))
        {
            File.Delete(instance.path);
            Debug.Log("Save eliminado en: " + instance.path);
        }   
    }
}
