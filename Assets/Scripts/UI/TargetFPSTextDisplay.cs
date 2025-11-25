using UnityEngine;
using TMPro;

public class TargetFPSTextDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;

    private float updateInterval = 0.2f; // cada 0.25 segundos
    private float timer = 0f;
    
    void Start()
    {
        UpdateFPSText(SettingsManager.Instance.TargetFPS);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            float fps = 1f / Time.unscaledDeltaTime;
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);
            timer = 0f;
        }
    }


    public void UpdateFPSText(int fps)
    {
        fpsText.text = "FPS: " + (fps == -1 ? "Unlimited" : fps.ToString());
    }
}
