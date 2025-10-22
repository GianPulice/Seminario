using TMPro;

public enum InteractionMode
{
    Press, Hold     
}

public interface IInteractable 
{
    InteractionMode InteractionMode { get; }

    bool TryGetInteractionMessage(out string message);
    void Interact(bool isPressed); 

    void ShowOutline();

    void HideOutline();
}
