using System.Collections;
using UnityEngine;

public class WordWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;

    [Header("Victory Sounds")]
    public AudioSource sfxSource;
    public AudioClip[] victoryClips;

    bool subscribed;

    void OnEnable() => Subscribe();
    void Start() => Subscribe();     
    void OnDisable() => Unsubscribe();

    void Subscribe()
    {
        if (subscribed || WordManager.I == null) return;
        WordManager.I.OnWin += HandleWin;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || WordManager.I == null) return;
        WordManager.I.OnWin -= HandleWin;
        subscribed = false;
    }

    void HandleWin()
    {
        PlayRandomVictorySound();
        StartCoroutine(GoRanking());
    }

    void PlayRandomVictorySound()
    {
        if (sfxSource == null || victoryClips == null || victoryClips.Length == 0) return;
        sfxSource.PlayOneShot(victoryClips[Random.Range(0, victoryClips.Length)]);
    }

    IEnumerator GoRanking()
    {
        yield return new WaitForSeconds(delay);

        if (uiRouter != null) uiRouter.Show_upgRanking();
        else Debug.LogError("WordWinToRanking: uiRouter está NULL");
    }
}