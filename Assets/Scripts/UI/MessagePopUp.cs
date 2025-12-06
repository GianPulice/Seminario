using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePopUp : MonoBehaviour
{
    public static MessagePopUp Instance;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject panel;

    void Awake() => Instance = this;

    public static void Show(string msg)
    {
        Instance.messageText.text = msg;
        Instance.panel.SetActive(true);
        LeanTween.delayedCall(2f, () => Instance.panel.SetActive(false));
    }
}
