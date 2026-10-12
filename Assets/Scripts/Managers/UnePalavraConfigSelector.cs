using UnityEngine;
using UnityEngine.UI;

public class UnePalavraConfigSelector : MonoBehaviour
{
    [Header("Categoria (aparece desde o início)")]
    public GameObject categoriaTitulo;     
    public Button[] categoryButtons;       // CasaBTN, FrutaBTN, AnimalsBTN
    public string[] categoryValues = { "Casa", "Frutas", "Animais" };

    [Header("Dificuldade (só aparece após escolher categoria)")]
    public GameObject dificuldadeTitulo;   // dificuldadeTXT
    public Button[] difficultyButtons;     // 3BTN, 4BTN, 6BTN
    public int[] difficultyValues = { 3, 4, 6 };

    [Header("Jogar (só aparece após escolher dificuldade)")]
    public GameObject playButton;         

    [Header("Cores")]
    public Color normal = Color.white;
    public Color selected = new Color(0.55f, 0.45f, 1f);

    bool categoryChosen;
    bool difficultyChosen;

    void Start()
    {
   
        categoriaTitulo.SetActive(true);
        SetButtonsActive(categoryButtons, true);

        dificuldadeTitulo.SetActive(false);
        SetButtonsActive(difficultyButtons, false);

        playButton.SetActive(false);

        categoryChosen = false;
        difficultyChosen = false;
    }

    // ligar no OnClick: CasaBTN -> SelectCategory(0), FrutaBTN -> (1), AnimalsBTN -> (2)
    public void SelectCategory(int index)
    {
        Highlight(categoryButtons, index);
        UnePalavraGameManager.I.SetCategory(categoryValues[index]);

        if (!categoryChosen)
        {
            categoryChosen = true;
            dificuldadeTitulo.SetActive(true);
            SetButtonsActive(difficultyButtons, true);
        }
    }

    // ligar no OnClick: 3BTN -> SelectDifficulty(0), 4BTN -> (1), 6BTN -> (2)
    public void SelectDifficulty(int index)
    {
        Highlight(difficultyButtons, index);
        UnePalavraGameManager.I.SetDifficulty(difficultyValues[index]);

        if (!difficultyChosen)
        {
            difficultyChosen = true;
            playButton.SetActive(true);
        }
    }

    void SetButtonsActive(Button[] buttons, bool active)
    {
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(active);
        }
    }

    void Highlight(Button[] buttons, int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var img = buttons[i].GetComponent<Image>();
            img.color = normal;
            buttons[i].transform.localScale = Vector3.one;
        }

        var selectedBtn = buttons[index];
        selectedBtn.GetComponent<Image>().color = selected;
        selectedBtn.transform.localScale = Vector3.one * 1.1f;
    }
}