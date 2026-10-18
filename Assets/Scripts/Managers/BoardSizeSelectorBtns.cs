using UnityEngine;
using UnityEngine.UI;

public class BoardSizeSelectorBtns : MonoBehaviour
{
    public Button[] buttons;

    public Color normal = Color.white;

    // cor BEM visível (roxo estilo seu tema)
    public Color selected = new Color(0.55f, 0.45f, 1f);

    void Start()
    {
        ResetSelection(); // nenhum botão começa marcado
    }

    // Chamado também pelo ConfigProgressivo toda vez que a tela de config abre
    public void ResetSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = normal;
            buttons[i].transform.localScale = Vector3.one;
        }

        // volta pro padrão (2x2), sem nenhum botão marcado
        if (GameManager.I != null)
        {
            GameManager.I.rows = 2;
            GameManager.I.cols = 2;
        }
    }

    public void Select(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = normal;
            buttons[i].transform.localScale = Vector3.one;
        }

        // marca o clicado
        var selectedBtn = buttons[index];
        selectedBtn.GetComponent<Image>().color = selected;

        // leve zoom (fica mais bonito)
        selectedBtn.transform.localScale = Vector3.one * 1.1f;

        // configura tabuleiro
        switch (index)
        {
            case 0: GameManager.I.rows = 2; GameManager.I.cols = 2; break;
            case 1: GameManager.I.rows = 3; GameManager.I.cols = 2; break;
            case 2: GameManager.I.rows = 4; GameManager.I.cols = 2; break;
            case 3: GameManager.I.rows = 4; GameManager.I.cols = 3; break;
        }
    }
}