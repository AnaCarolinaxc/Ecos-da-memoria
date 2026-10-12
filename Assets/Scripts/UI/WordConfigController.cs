using UnityEngine;
using UnityEngine.UI;

// Tela "Preparação": escolhe categoria -> escolhe dificuldade -> Jogar.
public class WordConfigController : MonoBehaviour
{
    [Header("Refs")]
    public UIRouter uiRouter;
    public WordBoard board;

    [Header("Pastas dos temas em Assets/Resources/UnePalavra (mesma ordem dos botões)")]
    public string[] themeFolders = { "Casa", "Fruta", "Animais" };
    public Image[] themeButtonImages;

    [Header("Dificuldade (nº de pares, mesma ordem dos botões)")]
    public GameObject difficultyGroup;      // só aparece depois de escolher o tema
    public int[] difficultyValues = { 3, 4, 6 };
    public Image[] difficultyButtonImages;

    [Header("Botão Jogar")]
    public GameObject playButton;           // só aparece depois de escolher os dois

    [Header("Cores dos botões")]
    public Color idleColor = new Color(1f, 0.90f, 0.70f);
    public Color selectedColor = new Color(0.85f, 0.65f, 0.15f);

    int themeIndex = -1;
    int difficultyIndex = -1;

    void OnEnable()
    {
        themeIndex = -1;
        difficultyIndex = -1;
        Refresh();
    }

    // Ligar nos botões de tema: OnClick -> SelectTheme(0), (1), (2)
    public void SelectTheme(int index)
    {
        themeIndex = index;
        Refresh();
    }

    // Ligar nos botões de dificuldade: OnClick -> SelectDifficulty(0), (1), (2)
    public void SelectDifficulty(int index)
    {
        difficultyIndex = index;
        Refresh();
    }

    // Ligar no botão Jogar
    public void Play()
    {
        if (themeIndex < 0 || difficultyIndex < 0) return;
        if (themeIndex >= themeFolders.Length || difficultyIndex >= difficultyValues.Length) return;

        WordManager.I.SetConfig(themeFolders[themeIndex], difficultyValues[difficultyIndex]);

        uiRouter.Show_upgPlaying(); 
        Canvas.ForceUpdateCanvases(); // garante o layout do painel antes de gerar os itens
        board.StartGame();
    }

    void Refresh()
    {
        Paint(themeButtonImages, themeIndex);
        Paint(difficultyButtonImages, difficultyIndex);

        if (difficultyGroup != null) difficultyGroup.SetActive(themeIndex >= 0);
        if (playButton != null) playButton.SetActive(themeIndex >= 0 && difficultyIndex >= 0);
    }

    void Paint(Image[] images, int selectedIndex)
    {
        if (images == null) return;
        for (int i = 0; i < images.Length; i++)
            if (images[i] != null)
                images[i].color = (i == selectedIndex) ? selectedColor : idleColor;
    }
}