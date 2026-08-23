using UnityEngine;
using UnityEngine.UI;

public class ConfigProgressivo : MonoBehaviour
{
    [Header("Elementos a mostrar progressivamente")]
    public GameObject areaImagens;
    public GameObject btnAdicionarImagem;
    public GameObject btnIniciarJogo;

    [Header("Referência à galeria")]
    public GalleryUI gallery;

    [Header("Áudio")]
    public AudioSource uiSource;
    public AudioClip configSound;
    public float tempoFase1Inicio = 0f;
    public float tempoFase1Fim = 3f;
    public float tempoFase2Inicio = 3f;
    public float tempoFase2Fim = 21f;
    public float tempoFase3Inicio = 21f;
    public float tempoFase3Fim = 25f;

    void OnEnable()
{
    Debug.Log($"ConfigProgressivo OnEnable! sprites={gallery.GetAllSprites().Count}");
    areaImagens.SetActive(false);
    btnAdicionarImagem.SetActive(false);
    btnIniciarJogo.SetActive(false);

    // Se já tem imagens da sessão anterior, pula para fase final
    if (gallery.GetAllSprites().Count > 0)
    {
        areaImagens.SetActive(true);
        btnAdicionarImagem.SetActive(true);
        btnIniciarJogo.SetActive(true);
        TocarTrecho(tempoFase3Inicio, tempoFase3Fim);
    }
    else
    {
        TocarTrecho(tempoFase1Inicio, tempoFase1Fim);
    }
}

    public void OnTamanhoSelecionado()
    {
        areaImagens.SetActive(true);
        btnAdicionarImagem.SetActive(true);
        AtualizarBtnJogar();

        TocarTrecho(tempoFase2Inicio, tempoFase2Fim);
    }

    public void OnImagemAdicionada()
    {
        AtualizarBtnJogar();
    }

    private void AtualizarBtnJogar()
    {
        bool temImagem = gallery.GetAllSprites().Count > 0;
        if (temImagem && !btnIniciarJogo.activeSelf)
        {
            btnIniciarJogo.SetActive(true);
            TocarTrecho(tempoFase3Inicio, tempoFase3Fim);
        }
    }

    private void TocarTrecho(float inicio, float fim)
    {
        Debug.Log($"TocarTrecho: inicio={inicio} fim={fim} uiSource={uiSource} configSound={configSound}");
        if (uiSource == null || configSound == null) return;
        uiSource.Stop();
        uiSource.clip = configSound;
        uiSource.time = inicio;
        uiSource.Play();
        StopAllCoroutines();
        StartCoroutine(PararEm(fim));
    }

    private System.Collections.IEnumerator PararEm(float fim)
    {
        yield return new WaitUntil(() => uiSource.time >= fim);
        uiSource.Stop();
    }
}