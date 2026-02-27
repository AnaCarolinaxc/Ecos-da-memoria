using UnityEngine;

public class UIRouter : MonoBehaviour
{
    public GameObject initial;
    public GameObject select;
    public GameObject home;
    public GameObject config;
    public GameObject playing;
    public GameObject ranking;

    [Header("Áudio")]
    public AudioSource uiSource;
    public AudioClip homeSound;
    public AudioClip configSound;
    public AudioClip playingSound;
    public AudioClip rankingSound;

    void Start() => ShowInitial();

    public void ShowHome()    => ShowOnly(home, homeSound, true);
    public void ShowSelect()  => ShowOnly(select, null, false);
    public void ShowInitial()  => ShowOnly(initial, null, false);
    public void ShowConfig()  => ShowOnly(config, configSound, true);
    public void ShowPlaying() => ShowOnly(playing, playingSound, true);

    public void ShowRanking(bool playSound = true)
        => ShowOnly(ranking, rankingSound, playSound);

    void ShowOnly(GameObject target, AudioClip clip, bool playSound)
{
    // desativa telas
    initial.SetActive(false);
    select.SetActive(false);
    home.SetActive(false);
    config.SetActive(false);
    playing.SetActive(false);
    ranking.SetActive(false);


    // ativa a nova
    target.SetActive(true);


    // toca som da tela (se permitido)
    if (uiSource != null) uiSource.Stop();
    if (playSound && clip != null && uiSource != null)
        uiSource.PlayOneShot(clip);
}
}
