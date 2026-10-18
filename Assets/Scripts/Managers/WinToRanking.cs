using System.Collections;
using UnityEngine;

public class WinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;      // pausa DEPOIS do áudio de parabéns terminar

    [Header("Victory Sounds")]
    public AudioSource sfxSource;   // não é mais usado (o parabéns toca pelo UIRouter)
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
        StartCoroutine(GoRanking());
    }

    // Toca o parabéns, espera ele terminar (+ "delay") e só então abre a classificação.
    IEnumerator GoRanking()
    {
        float wait = delay;
        ConfettiBurst.Fire(); // dispara confete
        AudioClip clip = PickVictoryClip();
        if (clip != null && uiRouter != null)
        {
            uiRouter.PlayUI(clip);
            wait += clip.length;
        }

        yield return new WaitForSeconds(wait);

        if (uiRouter != null) uiRouter.ShowRanking(true);
        else Debug.LogError("WinToRanking: uiRouter está NULL");
    }

    AudioClip PickVictoryClip()
    {
        if (victoryClips == null || victoryClips.Length == 0) return null;
        return victoryClips[Random.Range(0, victoryClips.Length)];
    }
}