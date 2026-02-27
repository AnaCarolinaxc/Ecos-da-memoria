using TMPro;
using UnityEngine;

public class NotEnoughImagesPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;  
    [SerializeField] private TMP_Text messageText;

    void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    public void Show(string message)
    {
        messageText.text = message;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
