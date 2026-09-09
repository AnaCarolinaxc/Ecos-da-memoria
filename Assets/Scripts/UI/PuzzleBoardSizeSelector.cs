using UnityEngine;
using UnityEngine.UI;

public class PuzzleBoardSizeSelector : MonoBehaviour
{
    public Button[] buttons; // ordem: 2x2, 3x3, 4x4
    public Color normal = Color.white;
    public Color selected = new Color(0.9f, 0.5f, 0.75f);

    void Start() => Select(1); //já vem marcado no 3x3

    public void Select(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = normal;
            buttons[i].transform.localScale = Vector3.one;
        }

        buttons[index].GetComponent<Image>().color = selected;
        buttons[index].transform.localScale = Vector3.one * 1.1f;

        int size = index switch { 0 => 2, 1 => 3, 2 => 4, _ => 3 };
        PuzzleManager.I.rows = size;
        PuzzleManager.I.cols = size;
    }
}