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
    


    [Header("Áudio")]
    public AudioSource uiSource;
    public AudioClip homeSound;
    public AudioClip configSound;
    public AudioClip playingSound;
    public AudioClip rankingSound;
    public AudioClip selectSound;


    void Start() => ShowInitial();

    public void ShowHome()    => ShowOnly(home, homeSound, true);
    public void ShowSelect()  => ShowOnly(select, selectSound, true);
    public void ShowInitial()  => ShowOnly(initial, null, false);
    public void ShowConfig()  => ShowOnly(config, configSound, true);
    public void ShowPlaying() => ShowOnly(playing, playingSound, true);

    public void Show_pzgHome()    => ShowOnly(pzg_homePanel, null, false);
    public void Show_pzgConfig()  => ShowOnly(pzg_configPanel, null, false);
    public void Show_pzgPlaying() => ShowOnly(pzg_playingPanel, null, false);
    public void Show_pzgRanking()  => ShowOnly(pzg_rankingPanel, null, false);


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


    target.SetActive(true);

    if (uiSource != null) uiSource.Stop();
    if (playSound && clip != null && uiSource != null)
    {
        uiSource.clip = clip;
        uiSource.time = 0f;
        uiSource.Play(); // mudou de PlayOneShot para Play
    }
}
}
