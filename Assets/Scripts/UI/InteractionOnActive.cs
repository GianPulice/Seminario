using TMPro;
using UnityEngine;
using System.Collections;

public class InteractionOnActive : MonoBehaviour
{
    private Animator animator;
    private TextMeshProUGUI messageText;

    private static readonly int showStateHash = Animator.StringToHash("Show");

    private Coroutine showAnimationCoroutine;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        messageText = GetComponentInChildren<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (showAnimationCoroutine != null)
        {
            StopCoroutine(showAnimationCoroutine);
        }
        //Making sure the trigger is reset before setting it again
        animator.ResetTrigger("Hide");

        messageText.text = message;
        gameObject.SetActive(true);

        //Force entry to Show state
        showAnimationCoroutine = StartCoroutine(PlayShowAnimationAfterFrame());
    }
    private IEnumerator PlayShowAnimationAfterFrame()
    {
        yield return null;

        animator.Play(showStateHash, 0, 0f);

        showAnimationCoroutine = null;
    }
    public void Hide()
    {
        if (showAnimationCoroutine != null)
        {
            StopCoroutine(showAnimationCoroutine);
            showAnimationCoroutine = null;
            gameObject.SetActive(false);
            return;
        }
        // Only trigger hide animation if the game object is active
        if (gameObject.activeInHierarchy)
        {
            animator.SetTrigger("Hide");
            messageText.text = string.Empty;
        }
        else
        {
            gameObject.SetActive(false);
        }

    }
}
