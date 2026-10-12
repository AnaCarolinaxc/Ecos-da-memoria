using UnityEngine;
using UnityEngine.UI;

// Config do Quebra-Cabeça em etapas:
public class PuzzleConfigProgressivo : MonoBehaviour
{
    [Header("Aparecem depois de escolher a imagem")]
    public GameObject[] previewObjects;   
    public GameObject dificuldadeGroup;   

    [Header("Aparece depois de escolher a dificuldade")]
    public GameObject playButton;        

    [Header("Preview (os mesmos usados no PuzzleImagePicker)")]
    public Image previewImage;           
    public PuzzlePreviewGrid previewGrid; 

    bool tamanhoEscolhido;
    bool redesenhar;
    Sprite spriteInicial;
    Color corInicial;
    CanvasGroup[] previewGroups;

    void Awake()
    {
        // guarda o visual original do preview (antes de qualquer foto ser escolhida)
        if (previewImage != null)
        {
            spriteInicial = previewImage.sprite;
            corInicial = previewImage.color;
        }

        // PuzzlePreviewGrid precisa continuar ativo para funcionar.
        int n = previewObjects != null ? previewObjects.Length : 0;
        previewGroups = new CanvasGroup[n];
        for (int i = 0; i < n; i++)
        {
            if (previewObjects[i] == null) continue;
            var cg = previewObjects[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = previewObjects[i].AddComponent<CanvasGroup>();
            previewGroups[i] = cg;
        }
    }

    void OnEnable()
    {
        // recomeça do zero
        tamanhoEscolhido = false;

        if (PuzzleManager.I != null) PuzzleManager.I.SetImage(null);

        if (previewImage != null)
        {
            previewImage.sprite = spriteInicial;
            previewImage.color = corInicial;
        }

        redesenhar = true;

        Refresh();
    }

    // A galeria devolve a imagem de forma assíncrona (callback)
    void Update()
    {
        if (redesenhar)
        {
            redesenhar = false;
            if (previewGrid != null && PuzzleManager.I != null)
                previewGrid.Redraw(PuzzleManager.I.rows, PuzzleManager.I.cols); 
        }

        Refresh();
    }

    public void OnTamanhoEscolhido()
    {
        tamanhoEscolhido = true;
        Refresh();
    }

    void Refresh()
    {
        bool temImagem = PuzzleManager.I != null && PuzzleManager.I.HasImage();

        if (previewGroups != null)
            foreach (var cg in previewGroups)
                if (cg != null) cg.alpha = temImagem ? 1f : 0f;

        if (dificuldadeGroup != null && dificuldadeGroup.activeSelf != temImagem)
            dificuldadeGroup.SetActive(temImagem);

        bool podeJogar = temImagem && tamanhoEscolhido;
        if (playButton != null && playButton.activeSelf != podeJogar)
            playButton.SetActive(podeJogar);
    }
}