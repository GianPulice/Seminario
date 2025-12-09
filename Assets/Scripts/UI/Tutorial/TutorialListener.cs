using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialListener : Singleton<TutorialListener>
{
    private readonly Dictionary<TutorialType, bool> tutorialFlags = new();


    void Awake()
    {
        CreateSingleton(false);
        InitializeFlags();
        SubscribeToSaveSystemManagerEvents();
        SubscribeToTutorialEvents();
        OnLoadTutorials();
    }

    void OnDestroy()
    {
        UnsuscribeToSaveSystemManagerEvents();
        UnsubscribeFromTutorialEvents();
    }


    public bool AllTutorialsSeen()
    {
        return tutorialFlags.Values.All(flag => flag == false);
    }

    public bool TryTriggerManualTutorial(TutorialType type)
    {
        if (!tutorialFlags.ContainsKey(type))
            return false;

        if (tutorialFlags[type])
        {
            tutorialFlags[type] = false;        // marcar como mostrado
            return true;                        // permitir tutorial
        }

        return false; // ya fue visto
    }

    private void InitializeFlags()
    {
        foreach (TutorialType type in Enum.GetValues(typeof(TutorialType)))
            tutorialFlags[type] = true;
    }


    private void SubscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData += OnSaveTutorials;
        SaveSystemManager.OnLoadAllGameData += OnLoadTutorials;
    }

    private void UnsuscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData -= OnSaveTutorials;
        SaveSystemManager.OnLoadAllGameData -= OnLoadTutorials;
    }

    private void SubscribeToTutorialEvents()
    {
        TutorialTrigger.OnTryTriggerTutorial += CanTriggerTutorial;
        TutorialTrigger.OnTutorialTriggered += HandleTutorialTriggered;
    }

    private void UnsubscribeFromTutorialEvents()
    {
        TutorialTrigger.OnTryTriggerTutorial -= CanTriggerTutorial;
        TutorialTrigger.OnTutorialTriggered -= HandleTutorialTriggered;
    }

    private void OnSaveTutorials()
    {
        SaveData data = SaveSystemManager.LoadGame();

        data.tutorialFlags.Clear();

        foreach (var kvp in tutorialFlags)
        {
            data.tutorialFlags.Add(new TutorialFlagData
            {
                type = kvp.Key,
                flag = kvp.Value
            });
        }

        SaveSystemManager.SaveGame(data);
    }

    private void OnLoadTutorials()
    {
        SaveData data = SaveSystemManager.LoadGame();

        if (data.tutorialFlags == null || data.tutorialFlags.Count == 0)
            return;

        tutorialFlags.Clear();

        foreach (var entry in data.tutorialFlags)
        {
            tutorialFlags[entry.type] = entry.flag;
        }
    }

    private bool CanTriggerTutorial(TutorialType type)
    {
        if (!tutorialFlags.ContainsKey(type))
        {
            Debug.LogWarning($"TutorialType {type} not found in flags!");
            return false;
        }

        if (tutorialFlags[type])
        {
            tutorialFlags[type] = false;
            return true;
        }

        return false;
    }

    private void HandleTutorialTriggered(TutorialType type)
    {
        Debug.Log($"Tutorial triggered: {type}");
        TutorialScreensManager.Instance.SetTutorialType(type);
    }
}
