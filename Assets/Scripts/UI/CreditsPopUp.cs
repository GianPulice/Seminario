
using UnityEngine;

public class CreditsPopUp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBounce;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float animationDuration = 0.7f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void AnimateFromButton(Transform targetButton)
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(true);

        Vector3 endPosition = targetButton.position;
        float startX = -(rectTransform.rect.width * rectTransform.lossyScale.x);
        Vector3 startPosition = new Vector3(startX, endPosition.y, endPosition.z);
        
        transform.position = startPosition;
        transform.localScale = Vector3.zero;

        var scaleTween = LeanTween.scale(gameObject, Vector3.one, animationDuration / 2); // Aparecer rápido
        SetEase(scaleTween);
        var moveTween = LeanTween.move(gameObject, endPosition, animationDuration)
            .setDelay(0.1f);
        SetEase(moveTween);
    }
    public void CloseToLeft()
    {
        LeanTween.cancel(gameObject);

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        float endX = -(rectTransform.rect.width * rectTransform.lossyScale.x);
        Vector3 endPosition = new Vector3(endX, transform.position.y, transform.position.z);

        var moveTween = LeanTween.move(gameObject, endPosition, animationDuration);
        SetEase(moveTween);

        var scaleTween = LeanTween.scale(gameObject, Vector3.zero, animationDuration / 2)
            .setDelay(animationDuration / 2); // Que empiece a escalar a cero en la mitad de la animación de movimiento

        SetEase(scaleTween);

        scaleTween.setOnComplete(() => gameObject.SetActive(false));
    }
    private void SetEase(LTDescr tween)
    {
        if (easeType == LeanTweenType.animationCurve)
        {
            tween.setEase(animationCurve);
        }
        else
        {
            tween.setEase(easeType);
        }
    }
}
