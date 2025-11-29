using UnityEngine;
using UnityEngine.UI; 

public class ShineAnimator : MonoBehaviour
{
    [SerializeField] private float animationTime = 1.0f;
    [SerializeField] private float startValue = -0.5f; 
    [SerializeField] private float endValue = 1.5f;   

    private Material uiMaterial;

    void Awake()
    {
        if (GetComponent<Image>() != null)
        {
            uiMaterial = GetComponent<Image>().material;
        }
        else if (GetComponent<RawImage>() != null)
        {
            uiMaterial = GetComponent<RawImage>().material;
        }
        
    }

    public void PlayShine()
    {
        if (uiMaterial == null) return;

        uiMaterial.SetFloat("_ShineProgress", startValue);

        LeanTween.value(gameObject, startValue, endValue, animationTime)
            .setEase(LeanTweenType.linear)
            .setOnUpdate((float progress) =>
            {
                uiMaterial.SetFloat("_ShineProgress", progress);
            });
    }

    [ContextMenu("Test Shine")]
    private void TestShine()
    {
        PlayShine();
    }
}