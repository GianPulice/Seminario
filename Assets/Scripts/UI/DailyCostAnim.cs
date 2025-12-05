using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyCostAnim : MonoBehaviour
{
    [Header("Main Object Animation")]
    [SerializeField] private float scaleTime = 0.45f;
    [SerializeField] private LeanTweenType scaleEase = LeanTweenType.easeOutBack;
    [SerializeField] private Vector3 initialScale = new Vector3(0.4f, 0.4f, 0.4f);

    [Header("Timeline Text Sequence")]
    [SerializeField] private CanvasGroup[] textLines;   // Multiple text lines
    [SerializeField] private float textFadeTime = 0.3f;
    [SerializeField] private float delayBetweenLines = 0.15f;

    [Header("References")]
    [SerializeField] private RectTransform bigObject;
    [SerializeField] private GameObject button;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = bigObject.localScale;
        bigObject.localScale = initialScale;

        // Prepare all text lines
        foreach (var cg in textLines)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        button.SetActive(false);
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);

        LeanTween.cancel(bigObject.gameObject);

        // Reset
        foreach (var cg in textLines)
            cg.alpha = 0f;

        button.SetActive(false);
        bigObject.localScale = initialScale;

        // Pop panel
        LeanTween.scale(bigObject, originalScale, scaleTime)
            .setEase(scaleEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                StartTextTimeline();
            });
    }

    private void StartTextTimeline()
    {
        float delay = 0f;

        for (int i = 0; i < textLines.Length; i++)
        {
            CanvasGroup cg = textLines[i];

            LeanTween.alphaCanvas(cg, 1f, textFadeTime)
                .setDelay(delay)
                .setIgnoreTimeScale(true);

            delay += textFadeTime + delayBetweenLines;
        }

        // After all lines finish: Show button
        LeanTween.delayedCall(delay, ShowButton)
            .setIgnoreTimeScale(true);
    }

    private void ShowButton()
    {
        button.SetActive(true);

        button.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        LeanTween.scale(button, Vector3.one, 0.25f)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
    }

    public void HidePanel()
    {
        LeanTween.cancel(bigObject.gameObject);
        LeanTween.cancel(button);

        foreach (var cg in textLines)
            LeanTween.cancel(cg.gameObject);

        button.SetActive(false);

        // Shrink panel
        LeanTween.scale(bigObject, initialScale, 0.3f)
            .setEase(LeanTweenType.easeInBack)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);

                // Reset text for next appearance
                foreach (var cg in textLines)
                    cg.alpha = 0f;
            });
    }
}
