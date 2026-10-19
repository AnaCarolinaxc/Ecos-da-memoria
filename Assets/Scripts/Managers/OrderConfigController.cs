using UnityEngine;
using UnityEngine.UI;

public class OrderConfigController : MonoBehaviour
{
    public UIRouter uiRouter;

    [Header("Etapa 1 - ordem (0 = crescente, 1 = decrescente)")]
    public Image[] orderButtonImages;          // CresceBTN, DecresceBTN

    [Header("Etapa 2 - dificuldade (aparece depois da ordem)")]
    public GameObject difficultyGroup;         // dificuldadeTXT + botões 5, 10, 15
    public int[] difficultyValues = { 5, 10, 15 };
    public Image[] difficultyButtonImages;     // 5BTN, 10BTN, 15BTN

    [Header("Etapa 3 - jogar (aparece depois da dificuldade)")]
    public GameObject playButton;

    [Header("Áudio (1 por etapa, na ordem em que aparecem)")]
    public AudioClip etapa1Sound;              // ao abrir a tela: escolher a ordem
    public AudioClip etapa2Sound;              // depois de escolher a ordem: escolher a dificuldade
    public AudioClip etapa3Sound;              // depois de escolher a dificuldade: apertar jogar

    [Header("Cores dos botões")]
    public Color idleColor = Color.white;
    public Color selectedColor = new Color(0.62f, 0.78f, 0.45f);

    int orderIndex = -1;
    int difficultyIndex = -1;

    void OnEnable()
    {
        orderIndex = -1;
        difficultyIndex = -1;
        Refresh();

        PlayEtapa(etapa1Sound);   // etapa 1: escolher a ordem
    }

    public void SelectOrder(int index)
    {
        bool primeiraVez = orderIndex < 0;
        orderIndex = index;
        Refresh();

        if (primeiraVez) PlayEtapa(etapa2Sound);   // etapa 2: escolher a dificuldade
    }

    public void SelectDifficulty(int index)
    {
        bool primeiraVez = difficultyIndex < 0;
        difficultyIndex = index;
        Refresh();

        if (primeiraVez) PlayEtapa(etapa3Sound);   // etapa 3: apertar jogar
    }

    void PlayEtapa(AudioClip clip)
    {
        if (clip == null) return;
        if (uiRouter == null) uiRouter = FindFirstObjectByType<UIRouter>(FindObjectsInactive.Include);
        if (uiRouter != null) uiRouter.PlayUI(clip);
    }

    public void Play()
    {
        if (orderIndex < 0 || difficultyIndex < 0) return;
        if (OrderManager.I == null)
        {
            Debug.LogError("OrderConfigController: OrderManager não encontrado na cena.");
            return;
        }

        OrderManager.I.SetConfig(orderIndex == 0, difficultyValues[difficultyIndex]);

        if (uiRouter != null) uiRouter.Show_aogPlaying();
        else Debug.LogError("OrderConfigController: uiRouter está NULL");
    }

    void Refresh()
    {
        Paint(orderButtonImages, orderIndex);
        Paint(difficultyButtonImages, difficultyIndex);

        if (difficultyGroup != null) difficultyGroup.SetActive(orderIndex >= 0);
        if (playButton != null) playButton.SetActive(orderIndex >= 0 && difficultyIndex >= 0);
    }

    void Paint(Image[] images, int selected)
    {
        if (images == null) return;
        for (int i = 0; i < images.Length; i++)
            if (images[i] != null) images[i].color = (i == selected) ? selectedColor : idleColor;
    }
}