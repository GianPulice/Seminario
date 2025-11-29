using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : Singleton<OutlineManager>
{
    [SerializeField] private OutlineManagerData outlineManagerData;

    private Dictionary<GameObject, Outline> outlines = new();

    public OutlineManagerData OutlineManagerData { get => outlineManagerData; }


    void Awake()
    {
        CreateSingleton(true);
    }


    public void Register(GameObject obj)
    {
        if (obj == null) return;

        if (!outlines.ContainsKey(obj))
        {
            Outline outline = obj.AddComponent<Outline>();
            outline.OutlineWidth = 0;
            outline.OutlineColor = outlineManagerData.DefaultOutlineColor;
            outlines.Add(obj, outline);
        }
    }

    public void Unregister(GameObject obj)
    {
        if (obj == null) return;

        if (outlines.ContainsKey(obj))
        {
            outlines.Remove(obj);
        }
    }

    public void ShowWithDefaultColor(GameObject obj)
    {
        if (obj == null) return;

        if (outlines.TryGetValue(obj, out Outline outline))
        {
            outline.OutlineColor = outlineManagerData.DefaultOutlineColor;
            outline.OutlineWidth = outlineManagerData.ActiveWidth;
        }
    }

    public void ShowWithCustomColor(GameObject obj, Color color)
    {
        if (obj == null) return;

        if (outlines.TryGetValue(obj, out Outline outline))
        {
            outline.OutlineColor = color;
            outline.OutlineWidth = outlineManagerData.ActiveWidth;
        }
    }

    public void Hide(GameObject obj)
    {
        if (obj == null) return;

        if (outlines.TryGetValue(obj, out Outline outline))
        {
            outline.OutlineWidth = 0;
        }
    }
}
