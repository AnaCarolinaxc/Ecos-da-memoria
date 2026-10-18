using UnityEngine;

public class UIRouter : MonoBehaviour
{
    public GameObject initial;
    public GameObject select;
    public GameObject home;
    public GameObject config;
    public GameObject playing;
    public GameObject ranking;
    public GameObject pzg_homePanel;
    public GameObject pzg_configPanel;
    public GameObject pzg_playingPanel;
    public GameObject pzg_rankingPanel;
    public GameObject upg_homePanel;
    public GameObject upg_configPanel;
    public GameObject upg_playingPanel;
    public GameObject upg_rankingPanel;
    public GameObject aog_homePanel;
    public GameObject aog_configPanel;
    public GameObject aog_playingPanel;
    public GameObject aog_rankingPanel;


    [Header("Áudio")]
    public AudioSource uiSource;
    public AudioClip homeSound;       // primeira tela de CADA jogo (TelaInicialJogo) - igual nos 4 jogos
    public AudioClip configSound;
    public AudioClip playingSound;
    public AudioClip rankingSound;    // tela de classificação (classificacao) - vale para todos os jogos
    public AudioClip selectSound;     // tela de seleção de jogos (telaselecao)
    public AudioClip pzg_playingSound; // Quebra-cabeça: áudio ao começar a partida (ingame)
    public AudioClip aog_playingSound; // Acerte a Ordem: áudio ao começar a partida (ingame)
    public AudioClip upg_playingSound; // Una a Palavra: áudio ao começar a partida (ingame)


    void Start() => ShowInitial();

    public void ShowHome()    => ShowOnly(home, homeSound, true);
    public void ShowSelect()  => ShowOnly(select, selectSound, true);
    public void ShowInitial()  => ShowOnly(initial, null, false);
    public void ShowConfig()  => ShowOnly(config, configSound, true);
    public void ShowPlaying() => ShowOnly(playing, playingSound, true);

    public void Show_pzgHome()    => ShowOnly(pzg_homePanel, homeSound, true);
    public void Show_pzgConfig()  => ShowOnly(pzg_configPanel, null, false);
    public void Show_pzgPlaying() => ShowOnly(pzg_playingPanel, pzg_playingSound, true);
    public void Show_pzgRanking()        => ShowOnly(pzg_rankingPanel, rankingSound, true);
    // usado quando o jogador acabou de vencer: não toca a classificação por cima do som de vitória
    public void Show_pzgRankingSilent()  => ShowOnly(pzg_rankingPanel, null, false);

    public void Show_upgHome()    => ShowOnly(upg_homePanel, homeSound, true);
    public void Show_upgConfig()  => ShowOnly(upg_configPanel, null, false);
    public void Show_upgPlaying() => ShowOnly(upg_playingPanel, upg_playingSound, true);
    public void Show_upgRanking()        => ShowOnly(upg_rankingPanel, rankingSound, true);
    // usado quando o jogador acabou de vencer: não toca a classificação por cima do som de vitória
    public void Show_upgRankingSilent()  => ShowOnly(upg_rankingPanel, null, false);

    public void Show_aogHome()    => ShowOnly(aog_homePanel, homeSound, true);
    public void Show_aogConfig()  => ShowOnly(aog_configPanel, null, false);
    public void Show_aogPlaying() => ShowOnly(aog_playingPanel, aog_playingSound, true);
    public void Show_aogRanking()        => ShowOnly(aog_rankingPanel, rankingSound, true);
    // usado quando o jogador acabou de vencer: não toca a classificação por cima do som de vitória
    public void Show_aogRankingSilent()  => ShowOnly(aog_rankingPanel, null, false);

    public void ShowRanking(bool playSound = true)
        => ShowOnly(ranking, rankingSound, playSound);

void ShowOnly(GameObject target, AudioClip clip, bool playSound)
{
    initial.SetActive(false);
    select.SetActive(false);
    home.SetActive(false);
    config.SetActive(false);
    playing.SetActive(false);
    ranking.SetActive(false);

    pzg_homePanel.SetActive(false);
    pzg_configPanel.SetActive(false);
    pzg_playingPanel.SetActive(false);
    pzg_rankingPanel.SetActive(false);

    upg_homePanel.SetActive(false);
    upg_configPanel.SetActive(false);
    upg_playingPanel.SetActive(false);
    upg_rankingPanel.SetActive(false);

    aog_homePanel.SetActive(false);
    aog_configPanel.SetActive(false);
    aog_playingPanel.SetActive(false);
    aog_rankingPanel.SetActive(false);

    StopUI();                 // antes de ativar: o OnEnable da tela pode começar um áudio próprio
    target.SetActive(true);

    if (playSound) PlayUI(clip);
}

    // toca um áudio no uiSource (interrompe o que estiver tocando)
    public void PlayUI(AudioClip clip)
    {
        if (uiSource == null) return;
        uiSource.Stop();
        if (clip == null) return;
        uiSource.clip = clip;
        uiSource.time = 0f;
        uiSource.Play();
    }

    public void StopUI()
    {
        if (uiSource != null) uiSource.Stop();
    }
}