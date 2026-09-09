using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour, IPointerClickHandler
{
    public Image image;
    public GameObject selectionOutline;

    public int CorrectIndex { get; private set; }
    public int CurrentIndex { get; set; }
    public bool IsCorrect => CorrectIndex == CurrentIndex;

    PuzzleBoard board;

    public void Setup(PuzzleBoard board, int correctIndex, int currentIndex, Sprite sprite)
    {
        this.board = board;
        CorrectIndex = correctIndex;
        CurrentIndex = currentIndex;
        image.sprite = sprite;
        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        if (selectionOutline != null)
        {
            selectionOutline.SetActive(on);
        }
    }

    public void OnPointerClick(PointerEventData eventData) => board.OnPieceTapped(this);
}