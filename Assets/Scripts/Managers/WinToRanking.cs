using System.Collections;
using UnityEngine;

public class WinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;

    [Header("Victory Sounds")]
    public AudioSource sfxSource;
    public AudioClip[] victoryClips; // ← vários sons

    void OnEnable()
    {
        if (GameManager.I != null)
            GameManager.I.OnWin += HandleWin;
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.OnWin -= HandleWin;
    }

    void HandleWin()
    {
        PlayRandomVictorySound(); // toca na hora
        StartCoroutine(GoRanking());
    }

    void PlayRandomVictorySound()
    {
        if (victoryClips.Length == 0 || sfxSource == null)
            return;

        int index = Random.Range(0, victoryClips.Length);
        sfxSource.PlayOneShot(victoryClips[index]);
    }

    IEnumerator GoRanking()
{
    yield return new WaitForSeconds(delay);

    if (uiRouter != null)
        uiRouter.ShowRanking(false); 
    else
        Debug.LogError("WinToRanking: uiRouter está NULL");
}
}
