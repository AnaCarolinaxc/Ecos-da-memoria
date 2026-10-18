using System.Collections;
using UnityEngine;

public class PuzzleWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;           // pausa DEPOIS do áudio de parabéns terminar

    [Header("Victory Sounds")]
    public AudioClip[] victoryClips;     // áudios de parabéns (sorteia um)

    void Start()
    {
        PuzzleManager.I.OnWin += HandleWin;
    }

    void OnDisable()
    {
        PuzzleManager.I.OnWin -= HandleWin;
        Debug.Log("[PuzzleWinToRanking] Inscrito no evento OnWin");
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

        if (uiRouter == null) yield break;

        // se o jogador saiu da partida durante o parabéns, não puxa ele para a classificação
        if (uiRouter.pzg_playingPanel != null && !uiRouter.pzg_playingPanel.activeInHierarchy) yield break;

        uiRouter.Show_pzgRanking();
    }

    AudioClip PickVictoryClip()
    {
        if (victoryClips == null || victoryClips.Length == 0) return null;
        return victoryClips[Random.Range(0, victoryClips.Length)];
    }
}