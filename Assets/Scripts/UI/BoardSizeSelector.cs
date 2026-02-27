using UnityEngine;
using UnityEngine.UI;

public class BoardSizeSelector : MonoBehaviour
{
    public Button[] buttons;

    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 0.9f, 1f);

    void Start()
    {
        Select(buttons[0]); // seleciona o primeiro por padrão
    }

    public void Select(Button clicked)
    {
        // desmarca todos
        foreach (var b in buttons)
            b.GetComponent<Image>().color = normalColor;

        // marca o clicado
        clicked.GetComponent<Image>().color = selectedColor;
    }

    // ===== funções do tamanho =====

    public void Set2x2(Button b) { Select(b); GameManager.I.rows=2; GameManager.I.cols=2; }
    public void Set3x2(Button b) { Select(b); GameManager.I.rows=3; GameManager.I.cols=2; }
    public void Set4x2(Button b) { Select(b); GameManager.I.rows=4; GameManager.I.cols=2; }
    public void Set4x3(Button b) { Select(b); GameManager.I.rows=4; GameManager.I.cols=3; }
}
