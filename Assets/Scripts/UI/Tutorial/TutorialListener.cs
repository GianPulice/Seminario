using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialListener : Singleton<TutorialListener>
{
    private readonly Dictionary<TutorialType, bool> tutorialFlags = new();
    private void Awake()
    {
        CreateSingleton(false);
        InitializeFlags();
        SubscribeToTutorialEvents();
    }
    private void OnDestroy()
    {
        UnsubscribeFromTutorialEvents();
    }
    private void InitializeFlags()
    {
        foreach (TutorialType type in Enum.GetValues(typeof(TutorialType)))
            tutorialFlags[type] = true;
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
