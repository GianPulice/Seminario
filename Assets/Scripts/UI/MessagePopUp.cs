using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePopUp : MonoBehaviour
{
    public static MessagePopUp Instance;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform popupPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject backdrop;
    [SerializeField] private CloseButton closeButton;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private float autoHideTime = 0.85f;

    private Coroutine autoHideRoutine;

    public float AutoHideTime { get => autoHideTime; set => autoHideTime = value; }

    void Awake()
    {
        Instance = this;
        HideImmediate();

        if (closeButton != null)
            closeButton.GetComponent<CloseButton>()
                       .OnClickEvent?.AddListener(Hide);
    }
    public static void Show(string msg)
    {
        if (Instance == null)
        {
            Debug.LogError("No MessagePopUp in scene.");
            return;
        }

        Instance.ShowMessage(msg);
    }
    public void ShowMessage(string msg)
    {
        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        messageText.text = msg;

        ShowPopup();

        autoHideRoutine = StartCoroutine(AutoHide());
    }
    private void ShowPopup()
    {
        backdrop?.SetActive(true);

        canvasGroup.blocksRaycasts = true;
        popupPanel.localScale = Vector3.one * 0.85f;
        canvasGroup.alpha = 0f;

        // Fade in
        LeanTween.value(gameObject, 0f, 1f, fadeDuration)
            .setIgnoreTimeScale(true)
            .setOnUpdate(a => canvasGroup.alpha = a);

        // Scale pop
        LeanTween.scale(popupPanel, Vector3.one, scaleDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
    }

    public void Hide()
    {
        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        // Fade out
        LeanTween.value(gameObject, canvasGroup.alpha, 0f, fadeDuration)
            .setIgnoreTimeScale(true)
            .setOnUpdate(a => canvasGroup.alpha = a)
            .setOnComplete(() => HideImmediate());

        // Scale down slightly
        LeanTween.scale(popupPanel, Vector3.one * 0.9f, scaleDuration)
            .setIgnoreTimeScale(true);
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        popupPanel.localScale = Vector3.one;
        backdrop?.SetActive(false);
    }

    IEnumerator AutoHide()
    {
        yield return new WaitForSecondsRealtime(autoHideTime);
        Hide();
    }
}
