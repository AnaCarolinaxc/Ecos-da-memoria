using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("Refs")]
    public GameObject front;
    public GameObject back;
    public Image frontImage;
    public Button button;

    [HideInInspector] public int pairId;
    [HideInInspector] public bool isMatched;

    GameBoard board;

    public void Init(GameBoard b, int id, Sprite sprite)
    {
        board = b;
        pairId = id;
        isMatched = false;

        frontImage.sprite = sprite;
        SetFaceUp(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        button.interactable = true;
    }

    void OnClick()
    {
        if (isMatched) return;
        if (board == null) return;
        board.OnCardClicked(this);
    }

    public void SetFaceUp(bool up)
    {
        front.SetActive(up);
        back.SetActive(!up);
    }

    public void SetInteractable(bool value)
    {
        if (button) button.interactable = value;
    }
}