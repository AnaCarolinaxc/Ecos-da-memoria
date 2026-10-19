using System.Collections;
using UnityEngine;

public class WordWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;      // pausa DEPOIS do áudio de parabéns terminar

    [Header("Victory Sounds")]
    public AudioSource sfxSource;   // não é mais usado (o parabéns toca pelo UIRouter)
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
        StartCoroutine(GoRanking());
    }

    // Toca o parabéns, espera ele terminar (+ "delay") e só então abre a classificação.
    IEnumerator GoRanking()
    {
        float wait = delay;

        if (uiRouter != null)
        {
            ConfettiBurst.Fire();   // confete explodindo na tela

            // deixa terminar o áudio da última palavra clicada antes de tocar o parabéns
            var board = FindFirstObjectByType<WordBoard>(FindObjectsInactive.Include);
            while (board != null && board.IsItemAudioPlaying) yield return null;

            AudioClip clip = PickVictoryClip();
            if (clip != null)
            {
                uiRouter.PlayUI(clip);
                wait += clip.length;
            }
        }

        yield return new WaitForSeconds(wait);

        if (uiRouter != null) uiRouter.Show_upgRanking();
        else Debug.LogError("WordWinToRanking: uiRouter está NULL");
    }

    AudioClip PickVictoryClip()
    {
        if (victoryClips == null || victoryClips.Length == 0) return null;
        return victoryClips[Random.Range(0, victoryClips.Length)];
    }
}