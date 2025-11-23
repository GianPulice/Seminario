using System.Collections;
using UnityEngine;
public class CameraSwitcher : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera[] cameras;
    [SerializeField] private CanvasGroup fadeOverlay;
    [Header("TIMERS")]
    [SerializeField] private float minSwitchTime = 3f;
    [SerializeField] private float maxSwitchTime = 8f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private LeanTweenType fadeEase = LeanTweenType.easeInOutQuad;

    private int currentCameraIndex = 0;
    void Start()
    {
        if (fadeOverlay != null)
            fadeOverlay.alpha = 0f;

        ActivateCamera(0);
        StartCoroutine(AutoSwitchRoutine());
    }

    private IEnumerator AutoSwitchRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSwitchTime, maxSwitchTime);

            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        if (fadeOverlay != null)
        {
            LeanTween.alphaCanvas(fadeOverlay, 1f, fadeDuration).setEase(fadeEase);
            yield return new WaitForSeconds(fadeDuration);
        }

        PickRandomCamera();

        if (fadeOverlay != null)
        {
            LeanTween.alphaCanvas(fadeOverlay, 0f, fadeDuration).setEase(fadeEase);
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    private void PickRandomCamera()
    {
        if (cameras.Length <= 1) return;

        int newIndex = currentCameraIndex;

        while (newIndex == currentCameraIndex)
        {
            newIndex = Random.Range(0, cameras.Length);
        }

        ActivateCamera(newIndex);
    }

    private void ActivateCamera(int indexToEnable)
    {
        currentCameraIndex = indexToEnable;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }
}
