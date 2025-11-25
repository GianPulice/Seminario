using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabTweenButton : GenericTweenButton
{
    [Space(2)]
    [Header("TabGroup")]
    [SerializeField] private TabGroup tabGroup;
    public void Initialize(TabGroup _TabGroup)
    {
        this.tabGroup = _TabGroup;
        behavior = ButtonBehavior.ToggleSelect;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (behavior == ButtonBehavior.HoverOnly) return;
        if (tabGroup != null)
        {
            tabGroup.OnTabSelected(this, true);
        }
        else
        {
            Debug.LogWarning("TaButton no tiene un group asignado", this);
        }
        OnClick?.Invoke();
    }
}
