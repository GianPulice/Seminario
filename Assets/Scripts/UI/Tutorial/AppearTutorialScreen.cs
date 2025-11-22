using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearTutorialScreen : MonoBehaviour
{
    [Header("animation settings")]
    [SerializeField] private float animTime = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
    [SerializeField] private Vector3 initialScale = new Vector3(0.8f, 0.8f, 0.8f);

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;
        
        canvasGroup.alpha = 0f;
        transform.localScale = initialScale;
       
    }
    public void ShowPannel()
    {
        LeanTween.cancel(gameObject);

        gameObject.SetActive(true);
   //     AudioManager.Instance.PlayOneShotSFX("TutorialAppear");
        canvasGroup.interactable = false;

        LeanTween.alphaCanvas(canvasGroup, 1f, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        LeanTween.scale(gameObject, originalScale, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
    }
    public void HidePanel()
    {
        LeanTween.cancel(gameObject);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.alphaCanvas(canvasGroup, 0f, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        LeanTween.scale(gameObject, initialScale, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
