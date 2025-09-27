using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text layerText;

    private void OnEnable()
    {
        PlayerDungeonHUD.OnLayerChanged += LayerTextUpdate;
    }

    private void OnDisable()
    {
        PlayerDungeonHUD.OnLayerChanged -= LayerTextUpdate;
    }

    private void LayerTextUpdate(int currentLayer)
    {
        if (layerText != null)
        {
            layerText.text = $"Layer {currentLayer}";
        }
    }
}
