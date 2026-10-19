using System.Collections;
using UnityEngine;

public class ConfigProgressivo : MonoBehaviour
{
    [Header("Elementos a mostrar progressivamente")]
    public GameObject areaImagens;
    public GameObject btnAdicionarImagem;
    public GameObject btnIniciarJogo;

    [Header("Referência à galeria")]
    public GalleryUI gallery;

    [Header("Reset ao abrir a tela")]
    [Tooltip("Objeto BoardSizeSelector (se vazio, o script procura sozinho)")]
    public BoardSizeSelectorBtns tamanhoSelector;
    [Tooltip("Se marcado, mantém as imagens escolhidas na rodada anterior")]
    public bool manterImagens = true;

    [Header("Áudio (um arquivo por etapa)")]
    public AudioSource uiSource;
    [Tooltip("1 - ao abrir a tela: escolher o tamanho")]
    public AudioClip somFase1;
    [Tooltip("2 - depois de escolher o tamanho: adicionar imagens")]
    public AudioClip somFase2;
    [Tooltip("3 - quando o botão de jogar aparece")]
    public AudioClip somFase3;

    bool tamanhoJaEscolhido;

    void OnEnable()
    {
        // 1) esconde tudo PRIMEIRO: só o tamanho do tabuleiro fica visível
        areaImagens.SetActive(false);
        btnAdicionarImagem.SetActive(false);
        btnIniciarJogo.SetActive(false);
        tamanhoJaEscolhido = false;

        // 2) tamanho do tabuleiro recomeça sem nada marcado
        if (tamanhoSelector == null)
            tamanhoSelector = FindObjectOfType<BoardSizeSelectorBtns>(true);
        if (tamanhoSelector != null)
            tamanhoSelector.ResetSelection();

        if (!manterImagens && gallery != null)
            gallery.Clear();

        // espera 1 frame: o UIRouter para o áudio logo depois de abrir o painel
        StartCoroutine(TocarDepoisDeUmFrame(somFase1));
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void OnTamanhoSelecionado()
    {
        areaImagens.SetActive(true);
        btnAdicionarImagem.SetActive(true);
        bool jogarLiberado = AtualizarBtnJogar(tocarSom: false);

        // narração só na primeira escolha de tamanho; trocar de tamanho não repete
        if (!tamanhoJaEscolhido)
        {
            tamanhoJaEscolhido = true;
            // se já tem imagens (mantidas), o próximo passo é jogar
            Tocar(jogarLiberado ? somFase3 : somFase2);
        }
    }

    public void OnImagemAdicionada()
    {
        AtualizarBtnJogar(tocarSom: true);
    }

    // retorna true se o botão de jogar está visível
    private bool AtualizarBtnJogar(bool tocarSom)
    {
        bool temImagem = gallery.GetAllSprites().Count > 0;
        if (temImagem && !btnIniciarJogo.activeSelf)
        {
            btnIniciarJogo.SetActive(true);
            if (tocarSom) Tocar(somFase3);
        }
        return btnIniciarJogo.activeSelf;
    }

    private IEnumerator TocarDepoisDeUmFrame(AudioClip clip)
    {
        yield return null;
        Tocar(clip);
    }

    private void Tocar(AudioClip clip)
    {
        if (uiSource == null || clip == null) return;
        uiSource.Stop();
        uiSource.clip = clip;
        uiSource.time = 0f;
        uiSource.Play();
    }
}