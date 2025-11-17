using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Which Tutorial Trigger Am I")]
    [SerializeField] private TutorialData tutorialData;
    [SerializeField] private TutorialType tutorialType;
    public static event Action<TutorialType> OnTutorialTriggered;
    public static event Func<TutorialType, bool> OnTryTriggerTutorial;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && tutorialData.ActivateTutorial)
        {
            bool canTrigger = OnTryTriggerTutorial?.Invoke(tutorialType) ?? true;
            if (canTrigger)
                OnTutorialTriggered?.Invoke(tutorialType);
        }
    }
}
