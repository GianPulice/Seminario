using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BookManagerUI : Singleton<BookManagerUI>
{
    /// <summary>
    /// Averiguar porque aveces tira errror y aveces no
    /// </summary>

    private List<IBookableUI> bookElementsUI = new List<IBookableUI>();

    private static event Action onHideOutlinesAndTextsFromInteractableElements;

    private int indexCurrentPanelOpen = 0;

    private bool isBookOpen = false;
    private bool isRightStickUsing = false;

    public static Action OnHideOutlinesAndTextsFromInteractableElements { get => onHideOutlinesAndTextsFromInteractableElements; set => onHideOutlinesAndTextsFromInteractableElements = value; }

    public bool IsBookOpen { get => isBookOpen; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToUpdateManagerEvent();
        SuscribeToPlayerControllerEvent();
    }

    void Start()
    {
        StartCoroutine(FindPanelsInHierarchy());
    }

    // Simulacion de Update
    void UpdateBookManagerUI()
    {
        ChangePanelIndex();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerControllerEvent();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateBookManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateBookManagerUI;
    }

    private void SuscribeToPlayerControllerEvent()
    {
        PlayerController.OnOpenOrCloseBook += OpenOrCloseBook;
    }

    private void UnsuscribeToPlayerControllerEvent()
    {
        PlayerController.OnOpenOrCloseBook -= OpenOrCloseBook;
    }

    private IEnumerator FindPanelsInHierarchy()
    {
        yield return new WaitForSecondsRealtime(3);

        bookElementsUI = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IBookableUI>().Where(p => ((MonoBehaviour)p).gameObject.scene.isLoaded).OrderBy(p => p.IndexPanel).ToList();
    }

    private void OpenOrCloseBook()
    {
        if (PauseManager.Instance.IsGamePaused) return;
        if (bookElementsUI == null) return;

        onHideOutlinesAndTextsFromInteractableElements?.Invoke();

        if (!isBookOpen)
        {
            InteractionManagerUI.Instance.ShowOrHideCenterPointUI(false);
            isBookOpen = true;
            EnabledNextPanel(indexCurrentPanelOpen);
            return;
        }

        else if (isBookOpen)
        {
            InteractionManagerUI.Instance.ShowOrHideCenterPointUI(true);
            isBookOpen = false;
            DisableCurrentPanel(indexCurrentPanelOpen);
            indexCurrentPanelOpen = 0;
            return;
        }
    }

    private void ChangePanelIndex()
    {
        if (!isBookOpen) return;
        if (PauseManager.Instance.IsGamePaused) return;
        if (bookElementsUI == null) return;

        float rightStickX = Input.GetAxis("RightStickHorizontal");

        if ((PlayerInputs.Instance.E() || rightStickX > 0.5f) && !isRightStickUsing)
        {
            isRightStickUsing = true;

            DisableCurrentPanel(indexCurrentPanelOpen);
            indexCurrentPanelOpen++;

            if (indexCurrentPanelOpen >= bookElementsUI.Count)
            {
                indexCurrentPanelOpen = 0;
            }

            EnabledNextPanel(indexCurrentPanelOpen);
        }

        else if ((PlayerInputs.Instance.Q() || rightStickX < -0.5f) && !isRightStickUsing)
        {
            isRightStickUsing = true;

            DisableCurrentPanel(indexCurrentPanelOpen);
            indexCurrentPanelOpen--;

            if (indexCurrentPanelOpen < 0)
            {
                indexCurrentPanelOpen = bookElementsUI.Count - 1;
            }

            EnabledNextPanel(indexCurrentPanelOpen);
        }

        if (Mathf.Abs(rightStickX) < 0.2f)
        {
            isRightStickUsing = false;
        }
    }

    private void DisableCurrentPanel(int currentIndex)
    {
        bookElementsUI[currentIndex].ClosePanel();
    }

    private void EnabledNextPanel(int currentIndex)
    {
        bookElementsUI[currentIndex].OpenPanel();
    }
}
